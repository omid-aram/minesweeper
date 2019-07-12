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
        #region Properties

        Game game;
        int gridWidth, gridHeight, maxColCount, maxRowCount, minColCount, minRowCount, minMinePercent, maxMinePercent,
            starCount = 9, greenFlagCount = 3, heartCount = 2;
        float screenXdpi, screenYdpi;
        GridLayout gridLayout;
        bool isAppInitialized, isFlagDefault, isBombOnAutoClick;
        ImageView btnToggleFlagDefault, btnPlus, btnStarToPlus, btnStarToHeart;
        ImageView[] timerDigitsImages, remainFlagsImages, starDigitsImages, plusDigitsImages, heartDigitsImages;
        char[] bombsDigits, timerDigits, starDigits, plusDigits, heartDigits;
        Timer timer;
        TextView /*txtTimer, txtGolden,*/ txtMessage;
        Button /*btnStar, /*btnGreenFlag, /*btnHeart, */btnNewGame, btnUseHeart, btnDontUseHeart;
        LinearLayout linearLayoutMessage, linearLayoutButtons, linearLayoutUseHeart;
        Point lastPressedPoint, lastOpenedPressed;
        ProgressBar prgSilverTimes, prgGoldenTimes;

        #endregion

        #region Methods
        private void crackCell(int r, int c)
        {
            if (!isInBoard(r, c)) return;

            if (game.BoardCells[r, c].Value == -1)
            {
                putGreenFlag(r, c);
            }
            else
            {
                if (game.BoardCells[r, c].Status == CellStatus.Flagged) toggleFlag(r, c);

                pressCell(r, c);
            }
        }
        private int minAroundPressedCellsValue(int r, int c)
        {
            var result = 10;

            if (isInBoard(r - 1, c - 1) && game.BoardCells[r - 1, c - 1].Status == CellStatus.Pressed) result = Math.Min(result, game.BoardCells[r - 1, c - 1].Value);
            if (isInBoard(r - 1, c - 0) && game.BoardCells[r - 1, c - 0].Status == CellStatus.Pressed) result = Math.Min(result, game.BoardCells[r - 1, c - 0].Value);
            if (isInBoard(r - 1, c + 1) && game.BoardCells[r - 1, c + 1].Status == CellStatus.Pressed) result = Math.Min(result, game.BoardCells[r - 1, c + 1].Value);
            if (isInBoard(r - 0, c - 1) && game.BoardCells[r - 0, c - 1].Status == CellStatus.Pressed) result = Math.Min(result, game.BoardCells[r - 0, c - 1].Value);
            if (isInBoard(r - 0, c + 1) && game.BoardCells[r - 0, c + 1].Status == CellStatus.Pressed) result = Math.Min(result, game.BoardCells[r - 0, c + 1].Value);
            if (isInBoard(r + 1, c - 1) && game.BoardCells[r + 1, c - 1].Status == CellStatus.Pressed) result = Math.Min(result, game.BoardCells[r + 1, c - 1].Value);
            if (isInBoard(r + 1, c - 0) && game.BoardCells[r + 1, c - 0].Status == CellStatus.Pressed) result = Math.Min(result, game.BoardCells[r + 1, c - 0].Value);
            if (isInBoard(r + 1, c + 1) && game.BoardCells[r + 1, c + 1].Status == CellStatus.Pressed) result = Math.Min(result, game.BoardCells[r + 1, c + 1].Value);

            return result;
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

            timerDigits = new char[5];
            timerDigitsImages = new ImageView[5];
            timerDigitsImages[0] = FindViewById<ImageView>(Resource.Id.timerDigit_h1);
            timerDigitsImages[1] = FindViewById<ImageView>(Resource.Id.timerDigit_h2);
            timerDigitsImages[2] = FindViewById<ImageView>(Resource.Id.timerDigit_col);
            timerDigitsImages[3] = FindViewById<ImageView>(Resource.Id.timerDigit_m1);
            timerDigitsImages[4] = FindViewById<ImageView>(Resource.Id.timerDigit_m2);

            bombsDigits = new char[3];
            remainFlagsImages = new ImageView[3];
            remainFlagsImages[0] = FindViewById<ImageView>(Resource.Id.remainFlagDigit100);
            remainFlagsImages[1] = FindViewById<ImageView>(Resource.Id.remainFlagDigit10);
            remainFlagsImages[2] = FindViewById<ImageView>(Resource.Id.remainFlagDigit1);

            starDigits = new char[2];
            starDigitsImages = new ImageView[2];
            starDigitsImages[0] = FindViewById<ImageView>(Resource.Id.dgtStar0);
            starDigitsImages[1] = FindViewById<ImageView>(Resource.Id.dgtStar1);

            plusDigits = new char[2];
            plusDigitsImages = new ImageView[2];
            plusDigitsImages[0] = FindViewById<ImageView>(Resource.Id.dgtPlus0);
            plusDigitsImages[1] = FindViewById<ImageView>(Resource.Id.dgtPlus1);

            heartDigits = new char[2];
            heartDigitsImages = new ImageView[2];
            heartDigitsImages[0] = FindViewById<ImageView>(Resource.Id.dgtHeart0);
            heartDigitsImages[1] = FindViewById<ImageView>(Resource.Id.dgtHeart1);

            isFlagDefault = false;
            setFlagDefaultButton();

            setBonusNumbers();

            isAppInitialized = true;
        }
        private void setFlagDefaultButton()
        {
            btnToggleFlagDefault.SetImageResource(isFlagDefault ? Resource.Drawable.switch_flag : Resource.Drawable.switch_target);
        }
        private void setImageDigits(ImageView[] imageViewArray, char[] digitArray, string value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                if (i < digitArray.Length && value[i] != digitArray[i])
                {
                    digitArray[i] = value[i];
                    setSevenSegmentImage(imageViewArray[i], value[i]);
                }
            }
            for (int i = value.Length; i < digitArray.Length; i++)
            {
                imageViewArray[i].Visibility = ViewStates.Gone;
            }
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

            if (islastGameDone && gameMinColCount < maxColCount)
            {
                gameMinColCount++;
            }

            var random = new Random();

            game.ColCount = random.Next(gameMinColCount, gameMaxColCount);
            var firstBtnWidth = (int)Math.Round((decimal)gridWidth / game.ColCount);
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

            game.NumbersMode = NumbersMode.Dot; //(NumbersMode)random.Next(1, 4);

            game.Status = GameStatus.Created;

            setImageDigits(timerDigitsImages, timerDigits, "00:00");
            prgGoldenTimes.Progress = 0;
            prgSilverTimes.Progress = 0;

            linearLayoutMessage.Visibility = ViewStates.Gone;
            linearLayoutButtons.Visibility = ViewStates.Visible;
            linearLayoutUseHeart.Visibility = ViewStates.Gone;
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
            game.SilverTimeSeconds = (int)Math.Round(game.GoldenTimeSeconds * 0.2);

            prgGoldenTimes.Max = game.GoldenTimeSeconds;
            prgSilverTimes.Max = game.SilverTimeSeconds;

            var goldenShare = (float)game.GoldenTimeSeconds / (game.GoldenTimeSeconds + game.SilverTimeSeconds);

            prgGoldenTimes.LayoutParameters = new LinearLayout.LayoutParams(0, -1, goldenShare);
            prgSilverTimes.LayoutParameters = new LinearLayout.LayoutParams(0, -1, 1 - goldenShare);

            prgGoldenTimes.Progress = prgGoldenTimes.Max;
            prgSilverTimes.Progress = prgSilverTimes.Max;
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
        private void pressCell(int r, int c, bool isAutoClick = true)
        {
            if (game.Status == GameStatus.Created)
            {
                createNewBoard(r, c);
            }

            if (!isInBoard(r, c) || game.BoardCells[r, c].Status != CellStatus.NotPressed) return;

            lastPressedPoint = new Point(r, c);

            openCellImage(r, c, isAutoClick);

            if (game.BoardCells[r, c].Value == -1)
            {
                if (heartCount > 0)
                {
                    isBombOnAutoClick = isAutoClick;
                    game.Status = GameStatus.Paused;
                    linearLayoutMessage.Visibility = ViewStates.Gone;
                    linearLayoutButtons.Visibility = ViewStates.Gone;
                    linearLayoutUseHeart.Visibility = ViewStates.Visible;
                }
                else
                {
                    gameOver();
                }
                return;
            }

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
                lastOpenedPressed = new Point(r, c);

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
                greenFlagCount++;
                starCount++;
            }
            else if (game.IsInSilverTime)
            {
                greenFlagCount++;
                starCount++;
            }
            else
            {
                starCount++;
            }
            setBonusNumbers();

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
            linearLayoutUseHeart.Visibility = ViewStates.Gone;
        }
        private void setBonusNumbers()
        {
            //btnStar.Text = $"{starCount}";
            //btnGreenFlag.Text = $"{greenFlagCount}";
            //btnHeart.Text = $"{heartCount}";
            var starValue = starCount > 99 ? "99" : starCount.ToString();
            setImageDigits(starDigitsImages, starDigits, starValue);

            btnStarToPlus.Visibility = (starCount >= 3) ? ViewStates.Visible : ViewStates.Gone;
            btnStarToHeart.Visibility = (starCount >= 5) ? ViewStates.Visible : ViewStates.Gone;

            var plusValue = greenFlagCount > 99 ? "99" : greenFlagCount.ToString();
            setImageDigits(plusDigitsImages, plusDigits, plusValue);
            btnPlus.Visibility = (greenFlagCount > 0) ? ViewStates.Visible : ViewStates.Gone; 

            var heartValue = heartCount > 99 ? "99" : heartCount.ToString();
            setImageDigits(heartDigitsImages, heartDigits, heartValue);
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
            var value = Math.Min(game.FlagRemainCount, 999).ToString("D3");
            setImageDigits(remainFlagsImages, bombsDigits, value);

            //var d100 = /*value[0] == '0' ? ' ' :*/ value[0];
            //var d10 = /*value[0] == '0' && value[1] == '0' ? ' ' :*/ value[1];
            //var d1 = value[2];

            //if (d100 != bombsDigits[0])
            //{
            //    bombsDigits[0] = d100;
            //    setSevenSegmentImage(remainFlagDigit100, d100);
            //}
            //if (d10 != bombsDigits[1])
            //{
            //    bombsDigits[1] = d10;
            //    setSevenSegmentImage(remainFlagDigit10, d10);
            //}
            //if (d1 != bombsDigits[2])
            //{
            //    bombsDigits[2] = d1;
            //    setSevenSegmentImage(remainFlagDigit1, d1);
            //}

            //var txtFlagRemainCount = FindViewById<TextView>(Resource.Id.textView1);
            //txtFlagRemainCount.Text = game.FlagRemainCount.ToString();
        }
        private void setSevenSegmentImage(ImageView imageView, char value)
        {
            //imageView.Visibility = (value == ' ') ? ViewStates.Gone : ViewStates.Visible;

            switch (value)
            {
                case '0':
                    imageView.SetImageResource(Resource.Drawable.seven_seg_0);
                    break;
                case '1':
                    imageView.SetImageResource(Resource.Drawable.seven_seg_1);
                    break;
                case '2':
                    imageView.SetImageResource(Resource.Drawable.seven_seg_2);
                    break;
                case '3':
                    imageView.SetImageResource(Resource.Drawable.seven_seg_3);
                    break;
                case '4':
                    imageView.SetImageResource(Resource.Drawable.seven_seg_4);
                    break;
                case '5':
                    imageView.SetImageResource(Resource.Drawable.seven_seg_5);
                    break;
                case '6':
                    imageView.SetImageResource(Resource.Drawable.seven_seg_6);
                    break;
                case '7':
                    imageView.SetImageResource(Resource.Drawable.seven_seg_7);
                    break;
                case '8':
                    imageView.SetImageResource(Resource.Drawable.seven_seg_8);
                    break;
                case '9':
                    imageView.SetImageResource(Resource.Drawable.seven_seg_9);
                    break;
                case '-':
                    imageView.SetImageResource(Resource.Drawable.seven_seg_minus);
                    break;
                case ':':
                    imageView.SetImageResource(Resource.Drawable.seven_seg_column);
                    break;
                default:
                    imageView.SetImageResource(Resource.Drawable.seven_seg_null);
                    break;
            }
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
            linearLayoutUseHeart.Visibility = ViewStates.Gone;
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
                    cellImage =
                        game.NumbersMode == NumbersMode.Farsi ? Resource.Drawable.box_1 :
                        game.NumbersMode == NumbersMode.English ? Resource.Drawable.box_en_1 :
                        Resource.Drawable.box_dot_1;
                    break;
                case 2:
                    cellImage =
                        game.NumbersMode == NumbersMode.Farsi ? Resource.Drawable.box_2 :
                        game.NumbersMode == NumbersMode.English ? Resource.Drawable.box_en_2 :
                        Resource.Drawable.box_dot_2;
                    break;
                case 3:
                    cellImage =
                        game.NumbersMode == NumbersMode.Farsi ? Resource.Drawable.box_3 :
                        game.NumbersMode == NumbersMode.English ? Resource.Drawable.box_en_3 :
                        Resource.Drawable.box_dot_3;
                    break;
                case 4:
                    cellImage =
                        game.NumbersMode == NumbersMode.Farsi ? Resource.Drawable.box_4 :
                        game.NumbersMode == NumbersMode.English ? Resource.Drawable.box_en_4 :
                        Resource.Drawable.box_dot_4;
                    break;
                case 5:
                    cellImage =
                        game.NumbersMode == NumbersMode.Farsi ? Resource.Drawable.box_5 :
                        game.NumbersMode == NumbersMode.English ? Resource.Drawable.box_en_5 :
                        Resource.Drawable.box_dot_5;
                    break;
                case 6:
                    cellImage =
                        game.NumbersMode == NumbersMode.Farsi ? Resource.Drawable.box_6 :
                        game.NumbersMode == NumbersMode.English ? Resource.Drawable.box_en_6 :
                        Resource.Drawable.box_dot_6;
                    break;
                case 7:
                    cellImage =
                        game.NumbersMode == NumbersMode.Farsi ? Resource.Drawable.box_7 :
                        game.NumbersMode == NumbersMode.English ? Resource.Drawable.box_en_7 :
                        Resource.Drawable.box_dot_7;
                    break;
                case 8:
                    cellImage =
                        game.NumbersMode == NumbersMode.Farsi ? Resource.Drawable.box_8 :
                        game.NumbersMode == NumbersMode.English ? Resource.Drawable.box_en_8 :
                        Resource.Drawable.box_dot_8;
                    break;
            }
            setCellImage(r, c, cellImage);

            game.BoardCells[r, c].Status = CellStatus.Pressed;
        }
        private bool isInBoard(int r, int c)
        {
            return (r >= 0 && c >= 0 && r < game.RowCount && c < game.ColCount);
        }

        #endregion

        #region Events
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

            //remainFlagDigit100 = FindViewById<ImageView>(Resource.Id.remainFlagDigit100);
            //remainFlagDigit10 = FindViewById<ImageView>(Resource.Id.remainFlagDigit10);
            //remainFlagDigit1 = FindViewById<ImageView>(Resource.Id.remainFlagDigit1);

            //txtTimer = FindViewById<TextView>(Resource.Id.txtTimer);
            //txtGolden = FindViewById<TextView>(Resource.Id.txtGolden);

            //btnStar = FindViewById<Button>(Resource.Id.btnStar);
            //btnStar.Click += BtnStar_Click;

            btnStarToPlus = FindViewById<ImageView>(Resource.Id.btnStarToPlus);
            btnStarToPlus.Click += btnStarToPlus_Click;
            btnStarToHeart = FindViewById<ImageView>(Resource.Id.btnStarToHeart);
            btnStarToHeart.Click += btnStarToHeart_Click;

            btnPlus = FindViewById<ImageView>(Resource.Id.btnPlus);
            btnPlus.Click += btnPlus_Click;

            //btnHeart = FindViewById<Button>(Resource.Id.btnHeart);

            btnUseHeart = FindViewById<Button>(Resource.Id.btnUseHeart);
            btnUseHeart.Click += BtnUseHeart_Click;

            btnDontUseHeart = FindViewById<Button>(Resource.Id.btnDontUseHeart);
            btnDontUseHeart.Click += BtnDontUseHeart_Click;

            linearLayoutButtons = FindViewById<LinearLayout>(Resource.Id.linearLayoutButtons);
            linearLayoutMessage = FindViewById<LinearLayout>(Resource.Id.linearLayoutMessage);
            linearLayoutUseHeart = FindViewById<LinearLayout>(Resource.Id.linearLayoutUseHeart);
            txtMessage = FindViewById<TextView>(Resource.Id.txtMessage);
            btnNewGame = FindViewById<Button>(Resource.Id.btnNewGame);
            btnNewGame.Click += BtnNewGame_Click;

            prgSilverTimes = FindViewById<ProgressBar>(Resource.Id.prgSilverTimes);
            prgGoldenTimes = FindViewById<ProgressBar>(Resource.Id.prgGoldenTimes);
        }
        private void BtnDontUseHeart_Click(object sender, EventArgs e)
        {
            gameOver();
        }
        private void BtnUseHeart_Click(object sender, EventArgs e)
        {
            if (heartCount <= 0)
            {
                Toast.MakeText(Application.Context, "جون نداری", ToastLength.Short).Show();
                return;
            }

            if (!isBombOnAutoClick)
            {
                putGreenFlag(lastPressedPoint.X, lastPressedPoint.Y);
            }
            else
            {
                var r = lastOpenedPressed.X;
                var c = lastOpenedPressed.Y;

                crackCell(r - 1, c - 1);
                crackCell(r - 1, c - 0);
                crackCell(r - 1, c + 1);
                crackCell(r - 0, c - 1);
                crackCell(r - 0, c + 1);
                crackCell(r + 1, c - 1);
                crackCell(r + 1, c - 0);
                crackCell(r + 1, c + 1);
            }

            heartCount--;
            game.Status = GameStatus.Playing;

            linearLayoutMessage.Visibility = ViewStates.Gone;
            linearLayoutButtons.Visibility = ViewStates.Visible;
            linearLayoutUseHeart.Visibility = ViewStates.Gone;

            setBonusNumbers();

            checkIsWin();
        }
        private void btnStarToPlus_Click(object sender, EventArgs e)
        {
            if (starCount >= 3)
            {
                starCount -= 3;
                greenFlagCount++;
            }
            else
            {
                Toast.MakeText(Application.Context, "ستاره کم داری", ToastLength.Short).Show();
            }
            setBonusNumbers();
        }
        private void btnStarToHeart_Click(object sender, EventArgs e)
        {
            if (starCount >= 5)
            {
                starCount -= 5;
                heartCount++;
            }
            else
            {
                Toast.MakeText(Application.Context, "ستاره کم داری", ToastLength.Short).Show();
            }
            setBonusNumbers();
        }
        private void btnPlus_Click(object sender, EventArgs e)
        {
            if (game.Status != GameStatus.Playing) return;

            if (greenFlagCount == 0 || game.FlagRemainCount <= 0)
            {
                Toast.MakeText(Application.Context, "پرچم نداری", ToastLength.Short).Show();
                return;
            }

            var targetPoint = new KeyValuePair<Point, int>(new Point(), 9);

            for (int r = 0; r < game.RowCount; r++)
            {
                for (int c = 0; c < game.ColCount; c++)
                {
                    if (game.BoardCells != null && game.BoardCells[r, c].Value == -1 && game.BoardCells[r, c].Status != CellStatus.Flagged)
                    {
                        var minValue = minAroundPressedCellsValue(r, c);
                        if (minValue <= targetPoint.Value)
                        {
                            targetPoint = new KeyValuePair<Point, int>(new Point(r, c), minValue);
                        }
                    }
                }
            }
            if (targetPoint.Value < 9)
            {
                putGreenFlag(targetPoint.Key.X, targetPoint.Key.Y);
                greenFlagCount--;
                setBonusNumbers();
                return;
            }
        }
        private void BtnNewGame_Click(object sender, EventArgs e)
        {
            newGame();
        }
        private void BtnToggleFlagDefault_Click(object sender, EventArgs e)
        {
            isFlagDefault = !isFlagDefault;
            setFlagDefaultButton();
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

                if (game != null && (game.Status == GameStatus.Playing || game.Status == GameStatus.Paused))
                {
                    game.GamePlayingTime = DateTime.Now - game.GameStartedTime;
                    var hours = game.GamePlayingTime.Hours;
                    var minutes = (hours * 60) + game.GamePlayingTime.Minutes;
                    var seconds = game.GamePlayingTime.Seconds;

                    if (minutes > 99)
                    {
                        minutes = 99;
                        seconds = 99;
                    }
                    //txtTimer.Text = $"{minutes:D2}:{seconds:D2}";
                    setImageDigits(timerDigitsImages, timerDigits, $"{minutes:D2}:{seconds:D2}");

                    var goldenProgress = prgGoldenTimes.Max - game.GamePlayingTime.TotalSeconds;
                    var silverProgress = prgSilverTimes.Max - (game.GamePlayingTime.TotalSeconds - prgGoldenTimes.Max);
                    //txtGolden.Text = game.IsInGoldenTime ? goldenProgress.ToString() : game.IsInSilverTime ? silverProgress.ToString() : string.Empty;

                    prgGoldenTimes.Progress = goldenProgress < 0 ? 0 : (int)Math.Round(goldenProgress);
                    prgSilverTimes.Progress = silverProgress < 0 ? 0 : (int)Math.Round(silverProgress);
                }
            });
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
        private void Button_Click(object sender, System.EventArgs e)
        {
            if (game.Status == GameStatus.Paused) return;

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
                if (game.BoardCells[r, c].Status == CellStatus.Pressed)
                {
                    openedPress(r, c);
                }
                else if (isFlagDefault)
                    toggleFlag(r, c);
                else
                    pressCell(r, c, false);
            }
        }
        private void Button_LongClick(object sender, View.LongClickEventArgs e)
        {
            if (game.Status == GameStatus.Paused) return;

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
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        #endregion
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
        public NumbersMode NumbersMode { get; set; }
        public DateTime GameStartedTime { get; set; }
        public TimeSpan GamePlayingTime { get; set; }
        public bool IsInGoldenTime => (int)GamePlayingTime.TotalSeconds < GoldenTimeSeconds;
        public bool IsInSilverTime => (int)GamePlayingTime.TotalSeconds < (GoldenTimeSeconds + SilverTimeSeconds);
    }
    public class BoardCell
    {
        public SByte Value { get; set; }
        public CellStatus Status { get; set; }
        public bool IsGreenFlag { get; set; }
        public bool IsNearByZero { get; set; }
    }

    public enum GameStatus
    {
        Created,
        Playing,
        Paused,
        Done,
        Fail,
    }

    public enum CellStatus
    {
        NotPressed,
        Pressed,
        Flagged,
    }

    public enum NumbersMode
    {
        Farsi = 1,
        English = 2,
        Dot = 3,
    }
}