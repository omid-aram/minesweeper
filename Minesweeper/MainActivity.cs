using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using System.Collections.Generic;
using Android.Views;
using Android.Content.PM;
using System;
using Xamarin.Essentials;
using System.Timers;
using System.Drawing;
using Android.Content;
using Timer = System.Timers.Timer;
using Android.Text;
using Android.Webkit;
//using Android.Support.V4.Content.Res;

namespace MinesweeperPlus
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : Activity
    {
        #region Properties

        Publisher publisher = Publisher.Bazaar;
        Game game;
        int gridWidth, gridHeight, maxColCount, maxRowCount, minColCount, minRowCount, minMinePercent, maxMinePercent,
            starCount = 3, greenFlagCount = 0, heartCount = 0;
        float screenXdpi, screenYdpi;
        GridLayout gridLayout;
        bool isAppInitialized, isFlagDefault, isBombOnAutoClick;
        ImageView btnToggleFlagDefault, btnPlus, btnStarToGift, btnNewGame, btnRestartGame, btnUseHeart, btnDontUseHeart,
                    btnStart, btnAppLike, btnAppShare, btnAppInfo, btnHome, imgBonusStar, imgBonusPlus, imgBonusHeart, imgSurprised;
        ImageView[] timerDigitsImages, remainFlagsImages, starDigitsImages, plusDigitsImages, heartDigitsImages;
        char[] bombsDigits, timerDigits, starDigits, plusDigits, heartDigits;
        Timer timer, blinkTimer;
        List<Tuple<ImageView, int, int>> blinkers;
        LinearLayout linearLayoutStop, linearLayoutPlaying, linearLayoutUseHeart, homeLayout, infoLayout;
        Point lastPressedPoint, lastOpenedPressed;
        ProgressBar prgSilverTimes, prgGoldenTimes;
        ImageButton btnInfoBack;
        WebView webView;

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
            var result = 9;

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
            //homeLayout = FindViewById<LinearLayout>(Resource.Id.homeLayout);
            gridWidth = homeLayout.Width;
            gridHeight = homeLayout.Height;

            //gridLayout = FindViewById<GridLayout>(Resource.Id.gridLayout);

            screenXdpi = Resources.DisplayMetrics.Xdpi;
            screenYdpi = Resources.DisplayMetrics.Ydpi;

            var minSizeCm = 0.5;
            maxColCount = (int)Math.Round(gridWidth / screenXdpi * 2.54 / minSizeCm);
            maxRowCount = (int)Math.Round(gridHeight / screenYdpi * 2.54 / minSizeCm);

            var maxSizeCm = 0.7;
            minColCount = (int)Math.Round(gridWidth / screenXdpi * 2.54 / maxSizeCm);
            minRowCount = (int)Math.Round(gridHeight / screenYdpi * 2.54 / maxSizeCm);

            minMinePercent = 15;
            maxMinePercent = 27;

            //calculating max bomb count
            var maxMineCount = (int)Math.Round((double)maxMinePercent * maxRowCount * maxColCount / 100);
            //Toast.MakeText(Application.Context, $"سعی کن برنده مرحله {En2Fa(maxMineCount.ToString())} بمب بشی", ToastLength.Long).Show();
            AlertDialog.Builder alertDiag = new AlertDialog.Builder(this);
            alertDiag.SetTitle("GAME GOAL");
            alertDiag.SetMessage($"Final Level has {maxMineCount} mines!\n\nTry to reach and win that.\n\nIt will give you 10 stars.");
            alertDiag.SetPositiveButton("OK", (senderAlert, args) =>
            {
                alertDiag.Dispose();
            });
            Dialog diag = alertDiag.Create();
            diag.Show();

            setFlagDefaultButton();

            //setBonusNumbers();

            isAppInitialized = true;
        }
        private void setFlagDefaultButton()
        {
            btnToggleFlagDefault.SetImageResource(isFlagDefault ? Resource.Drawable.btn_flag_to_target : Resource.Drawable.btn_target_to_flag);
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
                imageViewArray[i].Visibility = ViewStates.Visible;
            }
            for (int i = value.Length; i < digitArray.Length; i++)
            {
                imageViewArray[i].Visibility = ViewStates.Gone;
            }
        }
        private void newGame()
        {
            if (!isAppInitialized)
            {
                initApp();
            }

            homeLayout.Visibility = ViewStates.Gone;
            gridLayout.Visibility = ViewStates.Visible;
            infoLayout.Visibility = ViewStates.Gone;

            if (game == null)
            {
                game = new Game();
                game.ColCount = maxColCount;
                game.MinePercent = maxMinePercent;
            }
            //game.IsAutoFlagMode = true;

            StopBlinking();
            setBonusNumbers();

            var lastColCount = game.ColCount;
            var lastMinePercent = game.MinePercent;
            var islastGameDone = (game.Status == GameStatus.Done);

            gridLayout.RemoveAllViews();

            gridLayout.Orientation = GridOrientation.Horizontal;

            var random = new Random();

            if (game.Status == GameStatus.Done || game.Status == GameStatus.Fail || game.MineCount == 0)
            {
                var gameMinColCount = islastGameDone ? lastColCount : minColCount;
                var gameMaxColCount = islastGameDone ? maxColCount : lastColCount;

                if (islastGameDone && gameMinColCount < maxColCount)
                {
                    gameMinColCount++;
                }

                game.ColCount = random.Next(gameMinColCount, gameMaxColCount);
            }
            var firstBtnWidth = (int)Math.Round((decimal)gridWidth / game.ColCount);
            if (game.ColCount < minColCount) { game.ColCount = minColCount; }
            if (game.ColCount > maxColCount) { game.ColCount = maxColCount; }

            var firstBtnHeight = (int)(firstBtnWidth * screenYdpi / screenXdpi);
            game.RowCount = (int)Math.Round((decimal)gridHeight / firstBtnHeight);
            if (game.RowCount < minRowCount) { game.RowCount = minRowCount; }
            if (game.RowCount > maxRowCount) { game.RowCount = maxRowCount; }

            if (game.Status == GameStatus.Done || game.Status == GameStatus.Fail || game.MineCount == 0)
            {
                var gameMinMinePercent = islastGameDone ? lastMinePercent : minMinePercent;
                var gameMaxMinePercent = islastGameDone ? maxMinePercent : lastMinePercent;

                if (islastGameDone && gameMinMinePercent < maxMinePercent)
                {
                    gameMinMinePercent++;
                }

                game.MinePercent = random.Next(gameMinMinePercent, gameMaxMinePercent);
                game.MineCount = (int)Math.Round((double)game.MinePercent * game.RowCount * game.ColCount / 100); //easy: 15%, medium: 21%, hard: 27%
            }
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

                    //button.Touch += Button_Touch;
                    button.Click += Button_Click;
                    button.LongClick += Button_LongClick;

                    gridLayout.AddView(button);
                }
            }

            game.Status = GameStatus.Created;

            setImageDigits(timerDigitsImages, timerDigits, "00:00");
            prgGoldenTimes.Progress = 0;
            prgSilverTimes.Progress = 0;

            btnNewGame.SetImageResource(Resource.Drawable.emoji_smiling);
            linearLayoutStop.Visibility = ViewStates.Visible;
            linearLayoutPlaying.Visibility = ViewStates.Gone;
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

            btnNewGame.SetImageResource(Resource.Drawable.emoji_smiling);
            linearLayoutStop.Visibility = ViewStates.Gone;
            linearLayoutPlaying.Visibility = ViewStates.Visible;
            linearLayoutUseHeart.Visibility = ViewStates.Gone;

            setBonusNumbers();

            game.GameStartedTime = DateTime.Now;
            timer.Interval = 1000;
            timer.Start();
        }
        private void setCellImage(int r, int c, int resImage, bool isBlinking = false)
        {
            if (!isInBoard(r, c) || game.BoardCells[r, c].Status == CellStatus.Pressed) return;

            var position = r * game.ColCount + c;
            var button = (ImageView)gridLayout.GetChildAt(position);

            button.SetImageResource(resImage);

            if (isBlinking)
            {
                StartBlinking(button);
            }
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
                    linearLayoutStop.Visibility = ViewStates.Gone;
                    linearLayoutPlaying.Visibility = ViewStates.Gone;
                    linearLayoutUseHeart.Visibility = ViewStates.Visible;

                    StartBlinking(imgSurprised);
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

            //checkAutoFlag();

            if (game.Status == GameStatus.Playing)
            {
                checkIsWin();
            }
        }

        //private void checkAutoFlag()
        //{
        //    if (!game.IsAutoFlagMode) return;

        //    for (int r = 0; r < game.RowCount; r++)
        //    {
        //        for (int c = 0; c < game.ColCount; c++)
        //        {
        //            if (game.BoardCells[r, c].IsAutoFlagChecked ||
        //                game.BoardCells[r, c].Status != CellStatus.Pressed ||
        //                game.BoardCells[r, c].Value <= 0)
        //                continue;

        //            var cellValue = game.BoardCells[r, c].Value;
        //            var notPressedPoints = new List<Point>();

        //            if (isInBoard(r - 1, c - 1) && game.BoardCells[r - 1, c - 1].Status != CellStatus.Pressed) notPressedPoints.Add(new Point(r - 1, c - 1));
        //            if (isInBoard(r - 1, c - 0) && game.BoardCells[r - 1, c - 0].Status != CellStatus.Pressed) notPressedPoints.Add(new Point(r - 1, c - 0));
        //            if (isInBoard(r - 1, c + 1) && game.BoardCells[r - 1, c + 1].Status != CellStatus.Pressed) notPressedPoints.Add(new Point(r - 1, c + 1));
        //            if (isInBoard(r - 0, c - 1) && game.BoardCells[r - 0, c - 1].Status != CellStatus.Pressed) notPressedPoints.Add(new Point(r - 0, c - 1));
        //            if (isInBoard(r - 0, c + 1) && game.BoardCells[r - 0, c + 1].Status != CellStatus.Pressed) notPressedPoints.Add(new Point(r - 0, c + 1));
        //            if (isInBoard(r + 1, c - 1) && game.BoardCells[r + 1, c - 1].Status != CellStatus.Pressed) notPressedPoints.Add(new Point(r + 1, c - 1));
        //            if (isInBoard(r + 1, c - 0) && game.BoardCells[r + 1, c - 0].Status != CellStatus.Pressed) notPressedPoints.Add(new Point(r + 1, c - 0));
        //            if (isInBoard(r + 1, c + 1) && game.BoardCells[r + 1, c + 1].Status != CellStatus.Pressed) notPressedPoints.Add(new Point(r + 1, c + 1));

        //            if (cellValue == notPressedPoints.Count)
        //            {
        //                foreach (var item in notPressedPoints)
        //                {
        //                    putGreenFlag(item.X, item.Y);
        //                    game.BoardCells[item.X, item.Y].IsAutoFlagChecked = true;
        //                }
        //            }
        //        }
        //    }

        //}

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
            StopBlinking();

            timer.Stop();
            game.GamePlayingTime = DateTime.Now - game.GameStartedTime;

            for (int r = 0; r < game.RowCount; r++)
            {
                for (int c = 0; c < game.ColCount; c++)
                {
                    if (game.BoardCells[r, c].Value == -1)
                    {
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

                StartBlinking(btnNewGame, imgBonusStar, imgBonusPlus, imgBonusHeart);
            }
            else if (game.IsInSilverTime)
            {
                greenFlagCount++;
                starCount++;

                StartBlinking(btnNewGame, imgBonusStar, imgBonusPlus);
            }
            else
            {
                starCount++;
                //var thread = new Thread(new ThreadStart(() => blinkImage(imgBonusStar, Resource.Drawable.redstar, 0, 100, true)));
                StartBlinking(btnNewGame, imgBonusStar);
            }

            //جایزه مرحله آخر
            if (game.GameLevelPercent == 100)
            {
                starCount += 9;
            }

            setBonusNumbers();

            linearLayoutStop.Visibility = ViewStates.Visible;
            linearLayoutPlaying.Visibility = ViewStates.Gone;
            linearLayoutUseHeart.Visibility = ViewStates.Gone;
        }

        private void StartBlinking(params ImageView[] imageViews)
        {
            //StopBlinking();

            blinkers = new List<Tuple<ImageView, int, int>>();

            foreach (var imageView in imageViews)
            {
                int mainImage = 0, blinkImage = 0;

                if (imageView == imgBonusStar)
                {
                    mainImage = Resource.Drawable.red_star;
                    blinkImage = Resource.Drawable.emoji_glasses;
                }
                else if (imageView == imgBonusPlus)
                {
                    mainImage = Resource.Drawable.red_plus;
                    blinkImage = Resource.Drawable.emoji_glasses;
                }
                else if (imageView == imgBonusHeart)
                {
                    mainImage = Resource.Drawable.red_heart;
                    blinkImage = Resource.Drawable.emoji_glasses;
                }
                else if (imageView == btnNewGame)
                {
                    if (game.Status == GameStatus.Done)
                    {
                        mainImage = Resource.Drawable.emoji_glasses;
                        blinkImage = Resource.Drawable.emoji_glasses_top;
                    }
                    if (game.Status == GameStatus.Fail)
                    {
                        mainImage = Resource.Drawable.emoji_sad;
                        blinkImage = Resource.Drawable.emoji_crying;
                    }
                }
                else if (imageView == imgSurprised)
                {
                    mainImage = Resource.Drawable.emoji_surprised_yellow;
                    blinkImage = Resource.Drawable.emoji_surprised_red;
                }
                else
                {
                    mainImage = Resource.Drawable.box_flag_green;
                    blinkImage = Resource.Drawable.box_flag;
                }

                blinkers.Add(new Tuple<ImageView, int, int>(imageView, mainImage, blinkImage));
            }

            blinkTimer.Start();
        }

        private void StopBlinking()
        {
            if (blinkers == null) blinkers = new List<Tuple<ImageView, int, int>>();

            foreach (var blinker in blinkers)
            {
                var imageView = blinker.Item1;
                var mainImage = blinker.Item2;

                imageView.SetImageResource(mainImage);
            }

            blinkers = new List<Tuple<ImageView, int, int>>();

            blinkTimer.Stop();
        }

        private void setBonusNumbers()
        {
            var starValue = starCount > 99 ? "99" : starCount.ToString();
            setImageDigits(starDigitsImages, starDigits, starValue);

            btnStarToGift.Visibility = (starCount >= 3) ? ViewStates.Visible : ViewStates.Gone;
            //btnStarToGift2.Visibility = game.Status == GameStatus.Playing ? ViewStates.Gone : btnStarToGift.Visibility;

            var plusValue = greenFlagCount > 99 ? "99" : greenFlagCount.ToString();
            setImageDigits(plusDigitsImages, plusDigits, plusValue);
            btnPlus.Visibility = (greenFlagCount > 0 && game != null && game.Status == GameStatus.Playing) ? ViewStates.Visible : ViewStates.Gone;

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
        private void putGreenFlag(int r, int c, bool isBlinking = false)
        {
            if (game.BoardCells[r, c].Status != CellStatus.Flagged)
            {
                game.BoardCells[r, c].Status = CellStatus.Flagged;
                game.FlagRemainCount--;
            }
            game.BoardCells[r, c].IsGreenFlag = true;
            setCellImage(r, c, Resource.Drawable.box_flag_green, isBlinking);

            setFlagsView();
        }
        private void setFlagsView()
        {
            var flagCount = game.FlagRemainCount < 0 ? 0 : game.FlagRemainCount;
            var value = Math.Min(flagCount, 999).ToString("D3");
            setImageDigits(remainFlagsImages, bombsDigits, value);
        }
        private void setSevenSegmentImage(ImageView imageView, char value)
        {
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
            StopBlinking();

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

            linearLayoutStop.Visibility = ViewStates.Visible;
            linearLayoutPlaying.Visibility = ViewStates.Gone;
            linearLayoutUseHeart.Visibility = ViewStates.Gone;

            StartBlinking(btnNewGame);
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
                    cellImage = Resource.Drawable.box_dot_1;
                    break;
                case 2:
                    cellImage = Resource.Drawable.box_dot_2;
                    break;
                case 3:
                    cellImage = Resource.Drawable.box_dot_3;
                    break;
                case 4:
                    cellImage = Resource.Drawable.box_dot_4;
                    break;
                case 5:
                    cellImage = Resource.Drawable.box_dot_5;
                    break;
                case 6:
                    cellImage = Resource.Drawable.box_dot_6;
                    break;
                case 7:
                    cellImage = Resource.Drawable.box_dot_7;
                    break;
                case 8:
                    cellImage = Resource.Drawable.box_dot_8;
                    break;
            }
            setCellImage(r, c, cellImage);

            game.BoardCells[r, c].Status = CellStatus.Pressed;
        }
        private bool isInBoard(int r, int c)
        {
            return (r >= 0 && c >= 0 && r < game.RowCount && c < game.ColCount);
        }
        public static string En2Fa(string sNum)
        {
            if (string.IsNullOrEmpty(sNum))
                return string.Empty;

            var sFrNum = "";
            const string vInt = "1234567890";

            sNum = sNum.Trim();

            var mystring = sNum.ToCharArray(0, sNum.Length);

            for (var i = 0; i <= (mystring.Length - 1); i++)
                if (vInt.IndexOf(mystring[i]) == -1)
                    sFrNum += mystring[i];
                else
                    sFrNum += ((char)((int)mystring[i] + 1728));

            return sFrNum;
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

            blinkTimer = new Timer(400);
            blinkTimer.Elapsed += BlinkTimer_Elapsed;

            btnToggleFlagDefault = FindViewById<ImageView>(Resource.Id.btnToggleFlagDefault);
            btnToggleFlagDefault.Click += BtnToggleFlagDefault_Click;

            btnStarToGift = FindViewById<ImageView>(Resource.Id.btnStarToGift);
            btnStarToGift.Click += btnStarToGift_Click;

            btnPlus = FindViewById<ImageView>(Resource.Id.btnPlus);
            btnPlus.Click += btnPlus_Click;

            btnUseHeart = FindViewById<ImageView>(Resource.Id.btnUseHeart);
            btnUseHeart.Click += BtnUseHeart_Click;

            btnDontUseHeart = FindViewById<ImageView>(Resource.Id.btnDontUseHeart);
            btnDontUseHeart.Click += BtnDontUseHeart_Click;

            linearLayoutPlaying = FindViewById<LinearLayout>(Resource.Id.linearLayoutPlaying);
            linearLayoutStop = FindViewById<LinearLayout>(Resource.Id.linearLayoutStop);
            linearLayoutUseHeart = FindViewById<LinearLayout>(Resource.Id.linearLayoutUseHeart);

            btnNewGame = FindViewById<ImageView>(Resource.Id.btnNewGame);
            btnNewGame.Click += BtnNewGame_Click;

            btnRestartGame = FindViewById<ImageView>(Resource.Id.btnRestartGame);
            btnRestartGame.Click += BtnNewGame_Click;

            prgSilverTimes = FindViewById<ProgressBar>(Resource.Id.prgSilverTimes);
            prgGoldenTimes = FindViewById<ProgressBar>(Resource.Id.prgGoldenTimes);

            btnStart = FindViewById<ImageView>(Resource.Id.btnStart);
            btnStart.Click += btnStart_Click;

            btnAppLike = FindViewById<ImageView>(Resource.Id.btnAppLike);
            btnAppLike.Click += btnAppLike_Click;

            btnAppShare = FindViewById<ImageView>(Resource.Id.btnAppShare);
            btnAppShare.Click += btnAppShare_Click;

            btnAppInfo = FindViewById<ImageView>(Resource.Id.btnAppInfo);
            btnAppInfo.Click += btnAppInfo_Click;

            btnHome = FindViewById<ImageView>(Resource.Id.btnHome);
            btnHome.Click += btnHome_Click;

            imgBonusStar = FindViewById<ImageView>(Resource.Id.imgBonusStar);
            imgBonusPlus = FindViewById<ImageView>(Resource.Id.imgBonusPlus);
            imgBonusHeart = FindViewById<ImageView>(Resource.Id.imgBonusHeart);
            imgSurprised = FindViewById<ImageView>(Resource.Id.imgSurprised);

            var txtVersion = FindViewById<TextView>(Resource.Id.txtVersion);
            txtVersion.Text += AppInfo.VersionString;

            homeLayout = FindViewById<LinearLayout>(Resource.Id.homeLayout);
            gridLayout = FindViewById<GridLayout>(Resource.Id.gridLayout);
            infoLayout = FindViewById<LinearLayout>(Resource.Id.infoLayout);

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

            btnInfoBack = FindViewById<ImageButton>(Resource.Id.btnInfoBack);
            btnInfoBack.Click += btnInfoBack_Click;

            webView = FindViewById<WebView>(Resource.Id.webView);

            //SetWebViewClient with an instance of WebViewClientClass  
            webView.SetWebViewClient(new WebViewClientClass());

            //Enabled Javascript in Websettings  
            WebSettings websettings = webView.Settings;
            websettings.JavaScriptEnabled = true;

            setBonusNumbers();
        }

        private void btnHome_Click(object sender, EventArgs e)
        {
            homeLayout.Visibility = ViewStates.Visible;
            gridLayout.Visibility = ViewStates.Gone;
            infoLayout.Visibility = ViewStates.Gone;
        }

        private void btnAppInfo_Click(object sender, EventArgs e)
        {
            webView.LoadUrl("https://google.com");

            homeLayout.Visibility = ViewStates.Gone;
            gridLayout.Visibility = ViewStates.Gone;
            infoLayout.Visibility = ViewStates.Visible;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            newGame();
        }

        private void btnAppLike_Click(object sender, EventArgs e)
        {
            Android.App.AlertDialog.Builder alertDiag = new Android.App.AlertDialog.Builder(this);
            alertDiag.SetTitle("LIKE IT?");
            alertDiag.SetMessage("Please take a moment to rate it.");
            alertDiag.SetPositiveButton("Rate", (senderAlert, args) =>
            {
                var uri = Android.Net.Uri.Empty;
                var intent = new Intent();
                try
                {
                    switch (publisher)
                    {
                        case Publisher.PlayStore:
                            uri = Android.Net.Uri.Parse("market://details?id=" + AppInfo.PackageName);
                            intent = new Intent(Intent.ActionView, uri);
                            break;
                        case Publisher.Bazaar:
                            uri = Android.Net.Uri.Parse("bazaar://details?id=" + AppInfo.PackageName);
                            intent = new Intent(Intent.ActionEdit, uri);
                            Intent.SetPackage("com.farsitel.bazaar");
                            break;
                        case Publisher.Myket:
                            uri = Android.Net.Uri.Parse("myket://comment?id=" + AppInfo.PackageName);
                            intent = new Intent(Intent.ActionView, uri);
                            break;
                    }
                    StartActivity(intent);
                }
                catch (Exception)
                {
                    switch (publisher)
                    {
                        case Publisher.PlayStore:
                            uri = Android.Net.Uri.Parse("https://play.google.com/store/apps/details?id=" + AppInfo.PackageName);
                            break;
                        case Publisher.Bazaar:
                            uri = Android.Net.Uri.Parse("https://cafebazaar.ir/app/" + AppInfo.PackageName);
                            break;
                        case Publisher.Myket:
                            uri = Android.Net.Uri.Parse("https://myket.ir/app/" + AppInfo.PackageName);
                            break;
                    }
                    intent = new Intent(Intent.ActionView, uri);
                    StartActivity(intent);
                }
            });
            alertDiag.SetNegativeButton("No Thanks", (senderAlert, args) =>
            {
                alertDiag.Dispose();
            });

            Dialog diag = alertDiag.Create();
            diag.Show();
        }

        private void btnAppShare_Click(object sender, EventArgs e)
        {
            var appLink = string.Empty;
            var smsBody = string.Empty;
            switch (publisher)
            {
                case Publisher.PlayStore:
                    appLink = "https://play.google.com/store/apps/details?id=" + AppInfo.PackageName;
                    smsBody = "Minesweeper Plus\n" + appLink;
                    break;
                case Publisher.Bazaar:
                    appLink = "https://cafebazaar.ir/app/" + AppInfo.PackageName;
                    smsBody = "مین روب پلاس\n" + appLink;
                    break;
                case Publisher.Myket:
                    appLink = "https://myket.ir/app/" + AppInfo.PackageName;
                    smsBody = "مین روب پلاس\n" + appLink;
                    break;
            }
            Android.App.AlertDialog.Builder alertDiag = new Android.App.AlertDialog.Builder(this);
            alertDiag.SetTitle("SHARE WITH FRIENDS");
            alertDiag.SetMessage("Please share the goodness of Minesweeper Plus with your friends.");
            alertDiag.SetPositiveButton("OK", (senderAlert, args) =>
            {
                var smsIntent = new Intent(Intent.ActionSend);
                //smsIntent.SetData(Android.Net.Uri.Parse("sms:"));
                smsIntent.PutExtra(Intent.ExtraText/*"sms_body"*/, smsBody);
                smsIntent.SetType("text/plain");
                StartActivity(smsIntent);
                alertDiag.Dispose();
            });
            alertDiag.SetNegativeButton("No Thanks", (senderAlert, args) =>
            {
                alertDiag.Dispose();
            });
            Dialog diag = alertDiag.Create();
            diag.Show();
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

            linearLayoutStop.Visibility = ViewStates.Gone;
            linearLayoutPlaying.Visibility = ViewStates.Visible;
            linearLayoutUseHeart.Visibility = ViewStates.Gone;

            setBonusNumbers();

            checkIsWin();
        }
        private void btnStarToGift_Click(object sender, EventArgs e)
        {
            var _plusCount = starCount / 3;
            var _heartCount = starCount / 5;
            var nextPlus = (_plusCount + 1) * 3;
            var _nextPlus_HeartCount = nextPlus / 5;
            var nextHeart = (_heartCount + 1) * 5;
            var _nextHeart_PlusCount = nextHeart / 3;

            Android.App.AlertDialog.Builder alertDiag = new Android.App.AlertDialog.Builder(this);
            alertDiag.SetTitle("SELLING STARS");
            alertDiag.SetMessage($"You have\n{starCount} Stars = {_plusCount} Plus(s), {_heartCount} Heart(s)\n\n_______________________\nWant more? Play more!\n\n{nextPlus} Stars = {_plusCount + 1} Plus(s), {_nextPlus_HeartCount} Heart(s)\n{nextHeart} Stars = {_nextHeart_PlusCount} Plus(s), {_heartCount + 1} Heart(s)");
            alertDiag.SetPositiveButton("Sell", (senderAlert, args) =>
            {
                if (starCount >= 3)
                {
                    greenFlagCount += starCount / 3;
                    heartCount += starCount / 5;
                    starCount = Math.Min(starCount % 3, starCount % 5);

                    setBonusNumbers();
                }
            });
            alertDiag.SetNegativeButton("Play More", (senderAlert, args) =>
            {
                alertDiag.Dispose();
            });
            Dialog diag = alertDiag.Create();
            diag.Show();
        }
        private void btnPlus_Click(object sender, EventArgs e)
        {
            if (game == null || game.Status != GameStatus.Playing) return;

            if (greenFlagCount == 0 || game.FlagRemainCount <= 0)
            {
                Toast.MakeText(Application.Context, "پرچم نداری", ToastLength.Short).Show();
                return;
            }

            var targetPoint = new KeyValuePair<Point, int>(new Point(), 10);

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
            if (targetPoint.Value <= 9)
            {
                putGreenFlag(targetPoint.Key.X, targetPoint.Key.Y, true);
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
                    setImageDigits(timerDigitsImages, timerDigits, $"{minutes:D2}:{seconds:D2}");

                    var goldenProgress = prgGoldenTimes.Max - game.GamePlayingTime.TotalSeconds;
                    var silverProgress = prgSilverTimes.Max - (game.GamePlayingTime.TotalSeconds - prgGoldenTimes.Max);

                    prgGoldenTimes.Progress = goldenProgress < 0 ? 0 : (int)Math.Round(goldenProgress);
                    prgSilverTimes.Progress = silverProgress < 0 ? 0 : (int)Math.Round(silverProgress);
                }
            });
        }

        private void BlinkTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            RunOnUiThread(() =>
            {
                foreach (var blinker in blinkers)
                {
                    var imageView = blinker.Item1;
                    var mainImage = blinker.Item2;
                    var blinkImage = blinker.Item3;

                    if (imageView.Drawable.GetConstantState() == Resources.GetDrawable(mainImage, null).GetConstantState())
                    {
                        if (blinkImage > 0) imageView.SetImageResource(blinkImage);
                    }
                    else
                    {
                        if (mainImage > 0) imageView.SetImageResource(mainImage);
                    }
                }
            });
        }

        //private void Button_Touch(object sender, View.TouchEventArgs e)
        //{
        //    var button = (ImageView)sender;
        //    int position = gridLayout.IndexOfChild(button);

        //    var r = position / game.ColCount;
        //    var c = position % game.ColCount;

        //    switch (e.Event.Action & MotionEventActions.Mask)
        //    {
        //        case MotionEventActions.Down:
        //            txtTest.Text = $"Down: ({r}, {c}) - ({e.Event.RawX}, {e.Event.RawY})";
        //            break;
        //        case MotionEventActions.Move:
        //            //        showCandidatedCells(r, c);
        //            //Toast.MakeText(Application.Context, "MotionEventActions.Down", ToastLength.Short).Show();
        //            txtTest.Text = $"Move: ({r}, {c}) - ({e.Event.RawX}, {e.Event.RawY})";
        //            break;

        //        case MotionEventActions.Up:
        //            //        openedPress(r, c);
        //            //Toast.MakeText(Application.Context, "MotionEventActions.Up", ToastLength.Short).Show();
        //            txtTest.Text = $"Up: ({r}, {c}) - ({e.Event.RawX}, {e.Event.RawY})";
        //            button.PerformClick();
        //            break;
        //    }
        //}

        private void Button_Click(object sender, System.EventArgs e)
        {
            if (game.Status == GameStatus.Paused) return;

            StopBlinking();

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

            StopBlinking();

            Vibrate(50);

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
        public override void OnBackPressed()
        {
            if (homeLayout.Visibility == ViewStates.Visible || (game != null && game.Status == GameStatus.Playing))
            {
                base.OnBackPressed();
            }
            else
            {
                btnHome.PerformClick();
            }
        }
        private void btnInfoBack_Click(object sender, EventArgs e)
        {
            this.OnBackPressed();
        }

        #endregion
    }

    internal class WebViewClientClass : WebViewClient
    {
        //Give the host application a chance to take over the control when a new URL is about to be loaded in the current WebView.  
        public override bool ShouldOverrideUrlLoading(WebView view, string url)
        {
            view.LoadUrl(url);
            return true;
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
        public bool IsInSilverTime => (int)GamePlayingTime.TotalSeconds < (GoldenTimeSeconds + SilverTimeSeconds);
        public bool IsAutoFlagMode { get; set; }
    }
    public class BoardCell
    {
        public SByte Value { get; set; }
        public CellStatus Status { get; set; }
        public bool IsGreenFlag { get; set; }
        public bool IsNearByZero { get; set; }
        //public bool IsAutoFlagChecked { get; set; }
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

    public enum Publisher
    {
        PlayStore,
        Bazaar,
        Myket,
    }
}