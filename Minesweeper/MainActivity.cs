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
using System.Drawing;

namespace Minesweeper
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : AppCompatActivity
    {
        Game game;
        int gridWidth, gridHeight, maxColCount, maxRowCount, minColCount, minRowCount, minMinePercent, maxMinePercent, 
            starCount, greenFlagCount, heartCount;
        float screenXdpi, screenYdpi;
        GridLayout gridLayout;
        bool isAppInitialized, isFlagDefault;
        ImageView btnToggleFlagDefault;
        Timer timer;
        TextView txtTimer, txtGolden, txtMessage;
        Button btnStar, btnGreenFlag, btnHeart, btnNewGame;
        LinearLayout linearLayoutMessage, linearLayoutButtons;
        Point lastPressedPoint;
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
            txtGolden = FindViewById<TextView>(Resource.Id.txtGolden);
            btnStar = FindViewById<Button>(Resource.Id.btnStar);
            btnGreenFlag = FindViewById<Button>(Resource.Id.btnGreenFlag);
            btnGreenFlag.Click += BtnGreenFlag_Click;
            btnHeart = FindViewById<Button>(Resource.Id.btnHeart);

            linearLayoutButtons = FindViewById<LinearLayout>(Resource.Id.linearLayoutButtons);
            linearLayoutMessage = FindViewById<LinearLayout>(Resource.Id.linearLayoutMessage);
            txtMessage = FindViewById<TextView>(Resource.Id.txtMessage);
            btnNewGame = FindViewById<Button>(Resource.Id.btnNewGame);
            btnNewGame.Click += BtnNewGame_Click;
        }

        private void BtnGreenFlag_Click(object sender, EventArgs e)
        {
            if (greenFlagCount == 0)
            {
                Toast.MakeText(Application.Context, "پرچم نداری", ToastLength.Short).Show();
                return;
            }

            if (lastPressedPoint != null && game.BoardCells[lastPressedPoint.X, lastPressedPoint.Y].Value > 0)
            {

            }
        }

        private void BtnNewGame_Click(object sender, EventArgs e)
        {
            newGame();
        }

        private void initApp()
        {
            gridLayout = FindViewById<GridLayout>(Resource.Id.gridLayout);

            screenXdpi = Resources.DisplayMetrics.Xdpi;
            screenYdpi = Resources.DisplayMetrics.Ydpi;

            gridWidth = gridLayout.Width;
            gridHeight = gridLayout.Height;

            var minSizeCm = 0.5;
            maxColCount = (int)Math.Round(gridWidth / screenXdpi * 2.54 / minSizeCm);
            maxRowCount = (int)Math.Round(gridHeight / screenYdpi * 2.54 / minSizeCm);

            var maxSizeCm = 0.7;
            minColCount = (int)Math.Round(gridWidth / screenXdpi * 2.54 / maxSizeCm);
            minRowCount = (int)Math.Round(gridHeight / screenYdpi * 2.54 / maxSizeCm);

            minMinePercent = 15;
            maxMinePercent = 27;

            isAppInitialized = true;
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
                if (!isAppInitialized)
                {
                    initApp();

                    timer.Stop();
                    newGame();
                }

                if (game != null && game.Status == GameStatus.Playing)
                {
                    game.GamePlayingTime = DateTime.Now - game.GameStartedTime;
                    txtTimer.Text = $"{game.GamePlayingTime.Minutes:D2}:{game.GamePlayingTime.Seconds:D2}";
                    txtGolden.Text = game.IsInGoldenTime ? "G" : game.IsInSilverTime ? "S" : string.Empty;
                }
            });
        }

        private void newGame()
        {
            if (game == null)
            {
                game = new Game();
                game.ColCount = maxColCount;
                game.MinePercent = maxMinePercent;
            }

            var lastColCount = game.ColCount;
            var lastMinePercent = game.MinePercent;
            var islastGameDone = (game.Status == GameStatus.Done);

            gridLayout = FindViewById<GridLayout>(Resource.Id.gridLayout);

            gridLayout.RemoveAllViews();

            gridLayout.Orientation = GridOrientation.Horizontal;

            var gameMinColCount = islastGameDone ? lastColCount : minColCount;
            var gameMaxColCount = islastGameDone ? maxColCount : lastColCount;

            var random = new Random();
            var randColCount = random.Next(gameMinColCount, gameMaxColCount);

            var firstBtnWidth = (int)Math.Round((decimal)gridWidth / randColCount);
            game.ColCount = gridWidth / firstBtnWidth;
            if (game.ColCount < minColCount) { game.ColCount = minColCount; }
            if (game.ColCount > maxColCount) { game.ColCount = maxColCount; }

            var firstBtnHeight = (int)(firstBtnWidth * screenYdpi / screenXdpi);
            game.RowCount = (int)Math.Round((decimal)gridHeight / firstBtnHeight);
            if (game.RowCount < minRowCount) { game.RowCount = minRowCount; }
            if (game.RowCount > maxRowCount) { game.RowCount = maxRowCount; }

            var gameMinMinePercent = islastGameDone ? lastMinePercent : minMinePercent;
            var gameMaxMinePercent = islastGameDone ? maxMinePercent : lastMinePercent;

            if (islastGameDone && gameMinMinePercent < maxMinePercent)
            {
                gameMinMinePercent++;
            }

            game.MinePercent = random.Next(gameMinMinePercent, gameMaxMinePercent);
            game.MineCount = (int)Math.Round((double)game.MinePercent * game.RowCount * game.ColCount / 100); //easy: 15%, medium: 21%, hard: 27%
            game.FlagRemainCount = game.MineCount;
            setFlagsView();

            //calcutaing gameLevelPercent
            var colLevel = (float)(game.ColCount - minColCount) / (maxColCount - minColCount);
            var mineLevel = (float)(game.MinePercent - minMinePercent) / (maxMinePercent - minMinePercent);
            game.GameLevelPercent = (int)((colLevel + mineLevel) / 2 * 100);
            var prgGameLevel = FindViewById<ProgressBar>(Resource.Id.prgGameLevel);
            prgGameLevel.Progress = game.GameLevelPercent;

            gridLayout.RowCount = game.RowCount;
            gridLayout.ColumnCount = game.ColCount;

            var btnWidths = new int[game.ColCount];
            var btnHeights = new int[game.RowCount];

            var btnWidth_def = gridWidth / game.ColCount;
            var btnWidth_rem = gridWidth % game.ColCount;
            for (int i = 0; i < btnWidths.Length; i++)
            {
                btnWidths[i] = btnWidth_def + (btnWidth_rem-- > 0 ? 1 : 0);
            }

            var btnHeight_def = gridHeight / game.RowCount;
            var btnHeight_rem = gridHeight % game.RowCount;
            for (int i = 0; i < btnHeights.Length; i++)
            {
                btnHeights[i] = btnHeight_def + (btnHeight_rem-- > 0 ? 1 : 0);
            }

            for (int r = 0; r < game.RowCount; r++)
            {
                for (int c = 0; c < game.ColCount; c++)
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

            game.Status = GameStatus.Created;

            txtTimer.Text = "00:00";
            txtGolden.Text = "";

            linearLayoutMessage.Visibility = ViewStates.Gone;
            linearLayoutButtons.Visibility = ViewStates.Visible;
        }
        private void createNewBoard(int firstR, int firstC)
        {
            var gridChildCount = game.RowCount * game.ColCount;

            //Creating board cells array
            game.BoardCells = new BoardCell[game.RowCount, game.ColCount];
            for (int r = 0; r < game.RowCount; r++)
            {
                for (int c = 0; c < game.ColCount; c++)
                {
                    game.BoardCells[r, c] = new BoardCell
                    {
                        Status = CellStatus.NotPressed,
                    };
                }
            }

            //Locating mines
            var random = new Random();
            for (int i = 0; i < game.MineCount; i++)
            {
                var position = random.Next(0, gridChildCount - 1);

                var r = position / game.ColCount;
                var c = position % game.ColCount;

                if (game.BoardCells[r, c].Value == -1 ||
                    (r >= firstR - 1 && c >= firstC - 1 && r <= firstR + 1 && c <= firstC + 1))
                {
                    i--;
                }
                else
                {
                    addMine(r, c);
                }
            }

            //Calculating golden time
            calculateGoldenTime();

            startGame();
        }

        private void calculateGoldenTime()
        {
            for (int r = 0; r < game.RowCount; r++)
            {
                for (int c = 0; c < game.ColCount; c++)
                {
                    if (game.BoardCells[r, c].Value == 0)
                    {
                        game.BoardCells[r, c].IsNearByZero = true;

                        markAsNearByZero(r - 1, c - 1);
                        markAsNearByZero(r - 1, c - 0);
                        markAsNearByZero(r - 1, c + 1);
                        markAsNearByZero(r - 0, c - 1);
                        markAsNearByZero(r - 0, c + 1);
                        markAsNearByZero(r + 1, c - 1);
                        markAsNearByZero(r + 1, c - 0);
                        markAsNearByZero(r + 1, c + 1);
                    }
                }
            }

            var zeros = 0;
            for (int r = 0; r < game.RowCount; r++)
            {
                for (int c = 0; c < game.ColCount; c++)
                {
                    if (game.BoardCells[r, c].IsNearByZero) zeros++;
                }
            }

            game.GoldenTimeSeconds = ((game.RowCount * game.ColCount) - zeros) / 2;
            game.SilverTimeSeconds = (int)Math.Round(game.GoldenTimeSeconds * 1.2);
        }

        private void markAsNearByZero(int r, int c)
        {
            if (isInBoard(r, c))
            {
                game.BoardCells[r, c].IsNearByZero = true;
            }
        }

        private void startGame()
        {
            game.Status = GameStatus.Playing;
            game.GameStartedTime = DateTime.Now;
            timer.Interval = 1000;
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
            if (!isInBoard(r, c) || game.BoardCells[r, c].Status == CellStatus.Pressed) return;

            var position = r * game.ColCount + c;
            var button = (ImageView)gridLayout.GetChildAt(position);

            button.SetImageResource(resImage);
        }

        private void addMine(int r, int c)
        {
            game.BoardCells[r, c].Value = -1;

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
            if (isInBoard(r, c) && game.BoardCells[r, c].Value != -1)
            {
                game.BoardCells[r, c].Value++;
            }
        }

        private void Button_Click(object sender, System.EventArgs e)
        {
            var button = (ImageView)sender;
            int position = gridLayout.IndexOfChild(button);

            var r = position / game.ColCount;
            var c = position % game.ColCount;

            if (game.Status == GameStatus.Created)
            {
                pressCell(r, c, false);
            }

            if (game.Status == GameStatus.Playing)
            {
                if (isFlagDefault)
                    toggleFlag(r, c);
                else
                    pressCell(r, c, false);

                if (game.BoardCells[r, c].Status == CellStatus.Pressed)
                {
                    openedPress(r, c);
                }
            }
        }

        private void Button_LongClick(object sender, View.LongClickEventArgs e)
        {
            Vibrate(50);

            //TODO: 
            if (game.Status == GameStatus.Done || game.Status == GameStatus.Fail)
            {
                newGame();
                return;
            }

            var button = (ImageView)sender;
            int position = gridLayout.IndexOfChild(button);

            var r = position / game.ColCount;
            var c = position % game.ColCount;

            if (game.Status == GameStatus.Created)
            {
                pressCell(r, c, false);
            }

            if (game.Status == GameStatus.Playing)
            {
                if (isFlagDefault)
                    pressCell(r, c, false);
                else
                    toggleFlag(r, c);
            }
        }

        private void pressCell(int r, int c, bool isAutoClick = true)
        {
            if (game.Status == GameStatus.Created)
            {
                createNewBoard(r, c);
            }

            if (!isInBoard(r, c) || game.BoardCells[r, c].Status != CellStatus.NotPressed) return;

            lastPressedPoint = new Point(r, c);

            openCellImage(r, c, isAutoClick);

            if (game.BoardCells[r, c].Value == 0)
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

            if (game.BoardCells[r, c].Value == -1)
            {
                gameOver();
            }

            if (game.Status == GameStatus.Playing)
            {
                checkIsWin();
            }
        }

        private void openedPress(int r, int c)
        {
            var flaggedCount = 0;

            if (isInBoard(r - 1, c - 1) && game.BoardCells[r - 1, c - 1].Status == CellStatus.Flagged) flaggedCount++;
            if (isInBoard(r - 1, c - 0) && game.BoardCells[r - 1, c - 0].Status == CellStatus.Flagged) flaggedCount++;
            if (isInBoard(r - 1, c + 1) && game.BoardCells[r - 1, c + 1].Status == CellStatus.Flagged) flaggedCount++;
            if (isInBoard(r - 0, c - 1) && game.BoardCells[r - 0, c - 1].Status == CellStatus.Flagged) flaggedCount++;
            if (isInBoard(r - 0, c + 1) && game.BoardCells[r - 0, c + 1].Status == CellStatus.Flagged) flaggedCount++;
            if (isInBoard(r + 1, c - 1) && game.BoardCells[r + 1, c - 1].Status == CellStatus.Flagged) flaggedCount++;
            if (isInBoard(r + 1, c - 0) && game.BoardCells[r + 1, c - 0].Status == CellStatus.Flagged) flaggedCount++;
            if (isInBoard(r + 1, c + 1) && game.BoardCells[r + 1, c + 1].Status == CellStatus.Flagged) flaggedCount++;

            if (flaggedCount >= game.BoardCells[r, c].Value)
            {
                if (isInBoard(r - 1, c - 1) && game.BoardCells[r - 1, c - 1].Status != CellStatus.Pressed) pressCell(r - 1, c - 1);
                if (isInBoard(r - 1, c - 0) && game.BoardCells[r - 1, c - 0].Status != CellStatus.Pressed) pressCell(r - 1, c - 0);
                if (isInBoard(r - 1, c + 1) && game.BoardCells[r - 1, c + 1].Status != CellStatus.Pressed) pressCell(r - 1, c + 1);
                if (isInBoard(r - 0, c - 1) && game.BoardCells[r - 0, c - 1].Status != CellStatus.Pressed) pressCell(r - 0, c - 1);
                if (isInBoard(r - 0, c + 1) && game.BoardCells[r - 0, c + 1].Status != CellStatus.Pressed) pressCell(r - 0, c + 1);
                if (isInBoard(r + 1, c - 1) && game.BoardCells[r + 1, c - 1].Status != CellStatus.Pressed) pressCell(r + 1, c - 1);
                if (isInBoard(r + 1, c - 0) && game.BoardCells[r + 1, c - 0].Status != CellStatus.Pressed) pressCell(r + 1, c - 0);
                if (isInBoard(r + 1, c + 1) && game.BoardCells[r + 1, c + 1].Status != CellStatus.Pressed) pressCell(r + 1, c + 1);
            }
        }

        private void checkIsWin()
        {
            for (int r = 0; r < game.RowCount; r++)
            {
                for (int c = 0; c < game.ColCount; c++)
                {
                    if (game.BoardCells[r, c].Value != -1 && game.BoardCells[r, c].Status != CellStatus.Pressed) return;
                }
            }

            gameDone();
        }

        private void gameDone()
        {
            timer.Stop();
            game.GamePlayingTime = DateTime.Now - game.GameStartedTime;

            for (int r = 0; r < game.RowCount; r++)
            {
                for (int c = 0; c < game.ColCount; c++)
                {
                    if (game.BoardCells[r, c].Value == -1 /*&& game.BoardCells[r, c].Status != CellStatus.Flagged*/)
                    {
                        //toggleFlag(r, c);
                        putGreenFlag(r, c);
                    }
                }
            }

            game.Status = GameStatus.Done;

            if (game.IsInGoldenTime)
            {
                heartCount++;
                btnHeart.Text = $"H:{heartCount}";
            }
            else if (game.IsInSilverTime)
            {
                greenFlagCount++;
                btnGreenFlag.Text = $"F:{greenFlagCount}";
            }
            else
            {
                starCount++;
                btnStar.Text = $"S:{starCount}";
            }

            //Toast.MakeText(Application.Context, "Winner :D", ToastLength.Short).Show();
            //Android.App.AlertDialog.Builder dialog = new Android.App.AlertDialog.Builder(this);
            //Android.App.AlertDialog alert = dialog.Create();
            //alert.SetTitle("برنده شدی");
            //alert.SetMessage("بریم دست بعدی؟");
            //alert.SetButton("بریم", (c, ev) =>
            //{
            //    newGame();
            //});
            //alert.Show();
            txtMessage.Text = "برنده شدی";
            linearLayoutMessage.Visibility = ViewStates.Visible;
            linearLayoutButtons.Visibility = ViewStates.Gone;
        }

        private void toggleFlag(int r, int c)
        {
            if (!isInBoard(r, c) || game.BoardCells[r, c].Status == CellStatus.Pressed) return;

            if (game.BoardCells[r, c].Status == CellStatus.Flagged)
            {
                if (!game.BoardCells[r, c].IsGreenFlag)
                {
                    game.BoardCells[r, c].Status = CellStatus.NotPressed;
                    setCellImage(r, c, Resource.Drawable.box_up);
                    game.FlagRemainCount++;
                }
            }
            else if (game.FlagRemainCount > 0)
            {
                game.BoardCells[r, c].Status = CellStatus.Flagged;
                setCellImage(r, c, Resource.Drawable.box_flag);
                game.FlagRemainCount--;
            }

            setFlagsView();
        }

        private void putGreenFlag(int r, int c)
        {
            if (game.BoardCells[r, c].Status != CellStatus.Flagged)
            {
                game.BoardCells[r, c].Status = CellStatus.Flagged;
                game.FlagRemainCount--;
            }
            game.BoardCells[r, c].IsGreenFlag = true;
            setCellImage(r, c, Resource.Drawable.box_flag_green);

            setFlagsView();
        }
        private void setFlagsView()
        {
            var txtFlagRemainCount = FindViewById<TextView>(Resource.Id.textView1);
            txtFlagRemainCount.Text = game.FlagRemainCount.ToString();
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
            timer.Stop();
            game.GamePlayingTime = DateTime.Now - game.GameStartedTime;
            Vibrate(100);

            for (int r = 0; r < game.RowCount; r++)
            {
                for (int c = 0; c < game.ColCount; c++)
                {
                    openCellImage(r, c, true);
                    checkFlagCorrect(r, c);
                }
            }

            game.Status = GameStatus.Fail;
            //Toast.MakeText(Application.Context, "BOOOOOOM :(", ToastLength.Short).Show();
            //Android.App.AlertDialog.Builder dialog = new Android.App.AlertDialog.Builder(this);
            //Android.App.AlertDialog alert = dialog.Create();
            //alert.SetTitle("پات رفت رو مین");
            //alert.SetMessage("بریم یه دست دیگه؟");
            //alert.SetButton("بریم", (c, ev) =>
            //{
            //    newGame();
            //});
            //alert.Show();
            txtMessage.Text = "ترکیدی که";
            linearLayoutMessage.Visibility = ViewStates.Visible;
            linearLayoutButtons.Visibility = ViewStates.Gone;
        }

        private void checkFlagCorrect(int r, int c)
        {
            if (!isInBoard(r, c) || game.BoardCells[r, c].Status != CellStatus.Flagged) return;

            if (game.BoardCells[r, c].Value != -1)
            {
                setCellImage(r, c, Resource.Drawable.box_flag_wrong);
            }
        }

        private void openCellImage(int r, int c, bool isAutoClick)
        {
            if (!isInBoard(r, c) || game.BoardCells[r, c].Status == CellStatus.Pressed || game.BoardCells[r, c].Status == CellStatus.Flagged) return;

            int cellImage = Resource.Drawable.box_0;
            switch (game.BoardCells[r, c].Value)
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

            game.BoardCells[r, c].Status = CellStatus.Pressed;
        }

        private bool isInBoard(int r, int c)
        {
            return (r >= 0 && c >= 0 && r < game.RowCount && c < game.ColCount);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }

    public class Game
    {
        public int RowCount { get; set; }
        public int ColCount { get; set; }
        public int MinePercent { get; set; }
        public int MineCount { get; set; }
        public int FlagRemainCount { get; set; }
        public int GameLevelPercent { get; set; }
        public int GoldenTimeSeconds { get; set; }
        public int SilverTimeSeconds { get; set; }
        public BoardCell[,] BoardCells { get; set; }
        public GameStatus Status { get; set; }
        public DateTime GameStartedTime { get; set; }
        public TimeSpan GamePlayingTime { get; set; }
        public bool IsInGoldenTime => (int)GamePlayingTime.TotalSeconds < GoldenTimeSeconds;
        public bool IsInSilverTime => (int)GamePlayingTime.TotalSeconds < SilverTimeSeconds;
    }
    public class BoardCell
    {
        public SByte Value { get; set; }
        public CellStatus Status { get; set; }
        //public bool IsPressed { get; set; }
        //public bool IsFlagged { get; set; }
        public bool IsGreenFlag { get; set; }
        public bool IsNearByZero { get; set; }
    }

    public enum GameStatus
    {
        Created,
        Playing,
        Done,
        Fail,
    }

    public enum CellStatus
    {
        NotPressed,
        Pressed,
        Flagged,
        //GreenFlagged,
    }
}