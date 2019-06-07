using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using System.Collections.Generic;
using Android.Views;
using Android.Content.PM;
using System;
using Xamarin.Essentials;
using System.Timers;

namespace Minesweeper
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : AppCompatActivity
    {
        int gridWidth, gridHeight, maxColCount, maxRowCount, minColCount, minRowCount,
            rowCount, colCount, minePercent, mineCount, flagRemainCount;
        float screenXdpi, screenYdpi;
        GridLayout gridLayout;
        BoardCell[,] boardCells;
        bool isGameStarted, isGameFinished, isFlagDefault, isLastGameWinner;
        ImageView btnToggleFlagDefault;
        DateTime gameStartedTime;
        Timer timer;
        TextView txtTimer;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            timer = new Timer(1000);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();

            btnToggleFlagDefault = FindViewById<ImageView>(Resource.Id.btnToggleFlagDefault);
            btnToggleFlagDefault.Click += BtnToggleFlagDefault_Click;

            setFlagDefaultButton();

            txtTimer = FindViewById<TextView>(Resource.Id.txtTimer);
        }

        private void initGame()
        {
            gridLayout = FindViewById<GridLayout>(Resource.Id.gridLayout);

            screenXdpi = Resources.DisplayMetrics.Xdpi;
            screenYdpi = Resources.DisplayMetrics.Ydpi;

            gridWidth = gridLayout.Width; //Resources.DisplayMetrics.WidthPixels;
            gridHeight = gridLayout.Height; //Resources.DisplayMetrics.HeightPixels;

            var minSizeCm = 0.5;
            maxColCount = (int)Math.Round(gridWidth / screenXdpi * 2.54 / minSizeCm);
            maxRowCount = (int)Math.Round(gridHeight / screenYdpi * 2.54 / minSizeCm);

            var maxSizeCm = 0.7;
            minColCount = (int)Math.Round(gridWidth / screenXdpi * 2.54 / maxSizeCm);
            minRowCount = (int)Math.Round(gridHeight / screenYdpi * 2.54 / maxSizeCm);

            colCount = maxColCount;
            minePercent = 27;
        }

        private void BtnToggleFlagDefault_Click(object sender, EventArgs e)
        {
            isFlagDefault = !isFlagDefault;
            setFlagDefaultButton();
        }

        private void setFlagDefaultButton()
        {
            btnToggleFlagDefault.SetScaleType(ImageView.ScaleType.FitXy);
            btnToggleFlagDefault.SetImageResource(isFlagDefault ? Resource.Drawable.box_flag : Resource.Drawable.box_bomb);
        }

        public override void OnWindowFocusChanged(bool hasFocus)
        {
            base.OnWindowFocusChanged(hasFocus);

            if (hasFocus)
            {
                var uiOptions =
                  SystemUiFlags.HideNavigation |
                  SystemUiFlags.LayoutHideNavigation |
                  SystemUiFlags.LayoutFullscreen |
                  SystemUiFlags.Fullscreen |
                  SystemUiFlags.LayoutStable |
                  SystemUiFlags.ImmersiveSticky;

                Window.DecorView.SystemUiVisibility = (StatusBarVisibility)uiOptions;
            }
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            RunOnUiThread(() =>
            {
                if (!isGameStarted)
                {
                    initGame();

                    timer.Stop();
                    newGame();
                }
                else if (!isGameFinished)
                {
                    var timeSpan = DateTime.Now - gameStartedTime;
                    txtTimer.Text = $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
                }
            });
        }

        private void newGame()
        {
            gridLayout = FindViewById<GridLayout>(Resource.Id.gridLayout);

            //screenXdpi = Resources.DisplayMetrics.Xdpi;
            //screenYdpi = Resources.DisplayMetrics.Ydpi;

            //gridWidth = gridLayout.Width; //Resources.DisplayMetrics.WidthPixels;
            //gridHeight = gridLayout.Height; //Resources.DisplayMetrics.HeightPixels;

            //var minSizeCm = 0.5;
            //maxColCount = (int)Math.Round(gridWidth / screenXdpi * 2.54 / minSizeCm);
            //maxRowCount = (int)Math.Round(gridHeight / screenYdpi * 2.54 / minSizeCm);

            //var maxSizeCm = 0.7;
            //minColCount = (int)Math.Round(gridWidth / screenXdpi * 2.54 / maxSizeCm);
            //minRowCount = (int)Math.Round(gridHeight / screenYdpi * 2.54 / maxSizeCm);

            //colCount = maxColCount;
            //minePercent = 27;
            ////////////////////////////////

            isGameStarted = false;
            isGameFinished = false;

            gridLayout.RemoveAllViews();

            gridLayout.Orientation = GridOrientation.Horizontal;

            var gameMinColCount = isLastGameWinner ? colCount : minColCount;
            var gameMaxColCount = isLastGameWinner ? maxColCount : colCount;

            var random = new Random();
            var randColCount = random.Next(gameMinColCount, gameMaxColCount);

            var firstBtnWidth = (int)Math.Round((decimal)gridWidth / randColCount);
            colCount = gridWidth / firstBtnWidth;
            if (colCount < minColCount) { colCount = minColCount; }
            if (colCount > maxColCount) { colCount = maxColCount; }

            var firstBtnHeight = (int)(firstBtnWidth * screenYdpi / screenXdpi);
            rowCount = (int)Math.Round((decimal)gridHeight / firstBtnHeight);
            if (rowCount < minRowCount) { rowCount = minRowCount; }
            if (rowCount > maxRowCount) { rowCount = maxRowCount; }

            var gameMinMinePercent = isLastGameWinner ? minePercent : 15;
            var gameMaxMinePercent = isLastGameWinner ? 27 : minePercent;

            minePercent = random.Next(gameMinMinePercent, gameMaxMinePercent);
            mineCount = (int)Math.Round((double)minePercent * rowCount * colCount / 100); //easy: 15%, medium: 21%, hard: 27%
            flagRemainCount = mineCount;
            setFlagsView();

            gridLayout.RowCount = rowCount;
            gridLayout.ColumnCount = colCount;

            var btnWidths = new int[colCount];
            var btnHeights = new int[rowCount];

            var btnWidth_def = gridWidth / colCount;
            var btnWidth_rem = gridWidth % colCount;
            for (int i = 0; i < btnWidths.Length; i++)
            {
                btnWidths[i] = btnWidth_def + (btnWidth_rem-- > 0 ? 1 : 0);
            }

            var btnHeight_def = gridHeight / rowCount;
            var btnHeight_rem = gridHeight % rowCount;
            for (int i = 0; i < btnHeights.Length; i++)
            {
                btnHeights[i] = btnHeight_def + (btnHeight_rem-- > 0 ? 1 : 0);
            }

            for (int r = 0; r < rowCount; r++)
            {
                for (int c = 0; c < colCount; c++)
                {
                    var button = new ImageView(this);

                    button.LayoutParameters = new ViewGroup.LayoutParams(btnWidths[c], btnHeights[r]);

                    button.SetScaleType(ImageView.ScaleType.FitXy);

                    button.SetImageResource(Resource.Drawable.box_up);

                    button.Click += Button_Click;
                    button.LongClick += Button_LongClick;
                    //button.Touch += Button_Touch;
                    gridLayout.AddView(button);
                }
            }

            txtTimer.Text = "00:00";
        }
        private void createNewBoard(int firstR, int firstC)
        {
            var gridChildCount = rowCount * colCount;

            //Creating board cells array
            boardCells = new BoardCell[rowCount, colCount];
            for (int r = 0; r < rowCount; r++)
            {
                for (int c = 0; c < colCount; c++)
                {
                    boardCells[r, c] = new BoardCell();
                }
            }

            //Locating mines
            var random = new Random();
            for (int i = 0; i < mineCount; i++)
            {
                var position = random.Next(0, gridChildCount - 1);

                var r = position / colCount;
                var c = position % colCount;

                if (boardCells[r, c].Value == -1 ||
                    (r >= firstR - 1 && c >= firstC - 1 && r <= firstR + 1 && c <= firstC + 1))
                {
                    i--;
                }
                else
                {
                    addMine(r, c);
                }
            }

            startGame();
        }

        private void startGame()
        {
            isGameStarted = true;
            //Toast.MakeText(Application.Context, "Game started :) Mines count: " + mineCount, ToastLength.Short).Show();
            gameStartedTime = DateTime.Now;
            timer.Start();
        }

        //private void Button_Touch(object sender, View.TouchEventArgs e)
        //{
        //    var button = (ImageView)sender;
        //    int position = gridLayout.IndexOfChild(button);

        //    var r = position / colCount;
        //    var c = position % colCount;

        //    if (!boardCells[r, c].IsPressed || boardCells[r, c].Value == 0) return;

        //    switch (e.Event.Action & MotionEventActions.Mask)
        //    {
        //        case MotionEventActions.Down:
        //        case MotionEventActions.Move:
        //            showCandidatedCells(r, c);
        //            break;

        //        case MotionEventActions.Up:
        //            openedPress(r, c);
        //            break;

        //        case MotionEventActions.Cancel:
        //            releaseCandidatedCells(r, c);
        //            break;
        //    }
        //}

        //private void releaseCandidatedCells(int r, int c)
        //{
        //    throw new NotImplementedException();
        //}

        //private void showCandidatedCells(int r, int c)
        //{
        //    setCellImage(r - 1, c - 1, Resource.Drawable.box_0);
        //    setCellImage(r - 1, c - 0, Resource.Drawable.box_0);
        //    setCellImage(r - 1, c + 1, Resource.Drawable.box_0);
        //    setCellImage(r - 0, c - 1, Resource.Drawable.box_0);
        //    setCellImage(r - 0, c + 1, Resource.Drawable.box_0);
        //    setCellImage(r + 1, c - 1, Resource.Drawable.box_0);
        //    setCellImage(r + 1, c - 0, Resource.Drawable.box_0);
        //    setCellImage(r + 1, c + 1, Resource.Drawable.box_0);
        //}

        private void setCellImage(int r, int c, int resImage)
        {
            if (!isInBoard(r, c) || boardCells[r, c].IsPressed) return;

            var position = r * colCount + c;
            var button = (ImageView)gridLayout.GetChildAt(position);

            button.SetImageResource(resImage);
        }

        private void addMine(int r, int c)
        {
            boardCells[r, c].Value = -1;

            appendCellNumber(r - 1, c - 1);
            appendCellNumber(r - 1, c - 0);
            appendCellNumber(r - 1, c + 1);
            appendCellNumber(r - 0, c - 1);
            appendCellNumber(r - 0, c + 1);
            appendCellNumber(r + 1, c - 1);
            appendCellNumber(r + 1, c - 0);
            appendCellNumber(r + 1, c + 1);
        }

        private void appendCellNumber(int r, int c)
        {
            if (isInBoard(r, c) && boardCells[r, c].Value != -1)
            {
                boardCells[r, c].Value++;
            }
        }

        private void Button_Click(object sender, System.EventArgs e)
        {
            var button = (ImageView)sender;
            int position = gridLayout.IndexOfChild(button);

            var r = position / colCount;
            var c = position % colCount;

            if (isFlagDefault && isGameStarted)
                toggleFlag(r, c);
            else
                pressCell(r, c, false);

            if (isGameStarted && boardCells[r, c].IsPressed)
            {
                openedPress(r, c);
            }
        }

        private void Button_LongClick(object sender, View.LongClickEventArgs e)
        {
            Vibrate(50);

            if (isGameFinished)
            {
                newGame();
                return;
            }

            var button = (ImageView)sender;
            int position = gridLayout.IndexOfChild(button);

            var r = position / colCount;
            var c = position % colCount;

            if (isFlagDefault || !isGameStarted)
                pressCell(r, c, false);
            else
                toggleFlag(r, c);
        }

        private void pressCell(int r, int c, bool isAutoClick = true)
        {
            if (!isGameStarted)
            {
                createNewBoard(r, c);
            }

            if (!isInBoard(r, c) || boardCells[r, c].IsPressed || boardCells[r, c].IsFlagged) return;

            openCellImage(r, c, isAutoClick);

            if (boardCells[r, c].Value == 0)
            {
                pressCell(r - 1, c - 1);
                pressCell(r - 1, c - 0);
                pressCell(r - 1, c + 1);
                pressCell(r - 0, c - 1);
                pressCell(r - 0, c + 1);
                pressCell(r + 1, c - 1);
                pressCell(r + 1, c - 0);
                pressCell(r + 1, c + 1);
            }

            if (boardCells[r, c].Value == -1)
            {
                gameOver();
            }

            if (!isGameFinished)
            {
                checkIsWin();
            }
        }

        private void openedPress(int r, int c)
        {
            var flaggedCount = 0;

            if (isInBoard(r - 1, c - 1) && boardCells[r - 1, c - 1].IsFlagged) flaggedCount++;
            if (isInBoard(r - 1, c - 0) && boardCells[r - 1, c - 0].IsFlagged) flaggedCount++;
            if (isInBoard(r - 1, c + 1) && boardCells[r - 1, c + 1].IsFlagged) flaggedCount++;
            if (isInBoard(r - 0, c - 1) && boardCells[r - 0, c - 1].IsFlagged) flaggedCount++;
            if (isInBoard(r - 0, c + 1) && boardCells[r - 0, c + 1].IsFlagged) flaggedCount++;
            if (isInBoard(r + 1, c - 1) && boardCells[r + 1, c - 1].IsFlagged) flaggedCount++;
            if (isInBoard(r + 1, c - 0) && boardCells[r + 1, c - 0].IsFlagged) flaggedCount++;
            if (isInBoard(r + 1, c + 1) && boardCells[r + 1, c + 1].IsFlagged) flaggedCount++;

            if (flaggedCount >= boardCells[r, c].Value)
            {
                if (isInBoard(r - 1, c - 1) && !boardCells[r - 1, c - 1].IsPressed) pressCell(r - 1, c - 1);
                if (isInBoard(r - 1, c - 0) && !boardCells[r - 1, c - 0].IsPressed) pressCell(r - 1, c - 0);
                if (isInBoard(r - 1, c + 1) && !boardCells[r - 1, c + 1].IsPressed) pressCell(r - 1, c + 1);
                if (isInBoard(r - 0, c - 1) && !boardCells[r - 0, c - 1].IsPressed) pressCell(r - 0, c - 1);
                if (isInBoard(r - 0, c + 1) && !boardCells[r - 0, c + 1].IsPressed) pressCell(r - 0, c + 1);
                if (isInBoard(r + 1, c - 1) && !boardCells[r + 1, c - 1].IsPressed) pressCell(r + 1, c - 1);
                if (isInBoard(r + 1, c - 0) && !boardCells[r + 1, c - 0].IsPressed) pressCell(r + 1, c - 0);
                if (isInBoard(r + 1, c + 1) && !boardCells[r + 1, c + 1].IsPressed) pressCell(r + 1, c + 1);
            }
        }

        private void checkIsWin()
        {
            for (int r = 0; r < rowCount; r++)
            {
                for (int c = 0; c < colCount; c++)
                {
                    if (boardCells[r, c].Value != -1 && !boardCells[r, c].IsPressed) return;
                }
            }

            gameDone();
        }

        private void gameDone()
        {
            for (int r = 0; r < rowCount; r++)
            {
                for (int c = 0; c < colCount; c++)
                {
                    if (boardCells[r, c].Value == -1 && !boardCells[r, c].IsFlagged)
                    {
                        toggleFlag(r, c);
                    }
                }
            }

            isGameFinished = true;
            isLastGameWinner = true;
            Toast.MakeText(Application.Context, "Winner :D", ToastLength.Short).Show();
            timer.Stop();
        }

        private void toggleFlag(int r, int c)
        {
            if (!isInBoard(r, c) || boardCells[r, c].IsPressed) return;

            if (boardCells[r, c].IsFlagged)
            {
                boardCells[r, c].IsFlagged = false;
                setCellImage(r, c, Resource.Drawable.box_up);
                flagRemainCount++;
            }
            else if (flagRemainCount > 0)
            {
                boardCells[r, c].IsFlagged = true;
                setCellImage(r, c, Resource.Drawable.box_flag);
                flagRemainCount--;
            }

            setFlagsView();
        }

        private void setFlagsView()
        {
            var txtFlagRemainCount = FindViewById<TextView>(Resource.Id.textView1);
            txtFlagRemainCount.Text = flagRemainCount.ToString();
        }

        private void Vibrate(int duration)
        {
            try
            {
                // Use default vibration length
                Vibration.Vibrate(duration);
            }
            catch (FeatureNotSupportedException ex)
            {
                // Feature not supported on device
            }
            catch (Exception ex)
            {
                // Other error has occurred.
            }
        }

        private void gameOver()
        {
            Vibrate(100);

            for (int r = 0; r < rowCount; r++)
            {
                for (int c = 0; c < colCount; c++)
                {
                    openCellImage(r, c, true);
                    checkFlagCorrect(r, c);
                }
            }

            isGameFinished = true;
            isLastGameWinner = false;
            Toast.MakeText(Application.Context, "BOOOOOOM :(", ToastLength.Short).Show();
            timer.Stop();
        }

        private void checkFlagCorrect(int r, int c)
        {
            if (!isInBoard(r, c) || !boardCells[r, c].IsFlagged) return;

            if (boardCells[r, c].Value != -1)
            {
                setCellImage(r, c, Resource.Drawable.box_flag_wrong);
            }
        }

        private void openCellImage(int r, int c, bool isAutoClick)
        {
            if (!isInBoard(r, c) || boardCells[r, c].IsPressed || boardCells[r, c].IsFlagged) return;

            int cellImage = Resource.Drawable.box_0;
            switch (boardCells[r, c].Value)
            {
                case -1:
                    cellImage = isAutoClick ? Resource.Drawable.box_bomb : Resource.Drawable.box_bomb_red;
                    break;
                case 1:
                    cellImage = Resource.Drawable.box_1;
                    break;
                case 2:
                    cellImage = Resource.Drawable.box_2;
                    break;
                case 3:
                    cellImage = Resource.Drawable.box_3;
                    break;
                case 4:
                    cellImage = Resource.Drawable.box_4;
                    break;
                case 5:
                    cellImage = Resource.Drawable.box_5;
                    break;
                case 6:
                    cellImage = Resource.Drawable.box_6;
                    break;
                case 7:
                    cellImage = Resource.Drawable.box_7;
                    break;
                case 8:
                    cellImage = Resource.Drawable.box_8;
                    break;
            }
            setCellImage(r, c, cellImage);

            boardCells[r, c].IsPressed = true;
        }

        private bool isInBoard(int r, int c)
        {
            return (r >= 0 && c >= 0 && r < rowCount && c < colCount && !isGameFinished);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }

    public class BoardCell
    {
        public SByte Value { get; set; }
        public bool IsPressed { get; set; }
        public bool IsFlagged { get; set; }
    }
}