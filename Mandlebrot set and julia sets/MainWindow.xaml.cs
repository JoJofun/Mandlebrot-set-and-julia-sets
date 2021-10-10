using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NicholasHeerdtHW4
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int canvasX;
        int canvasY;

        uint[] savedMap;
        const uint BLACK = 4278190080;
        const uint RED = 4294901760;
        const uint GREEN = 4278255360;
        const uint BLUE = 4278190335;
        const uint WHITE = uint.MaxValue;

        double uYMin = -2.0; //I know the min is supposed to be 2 and max is -2, but I did this to fix a bug
        double uYMax = 2.0;
        double uXMin = -2.0;
        double uXMax = 2.0;

        bool isMandle = true;//says if whats being displayed is a mandlebrot set or not

        double a = -1.25;
        double b = 0.11;


        int limit = 100;
        double upBound = 30;



        uint[] copyMap;

        bool isStillDown = false;


        double savedMouseX = 0;
        double savedMouseY = 0;

        public MainWindow()
        {
            InitializeComponent();

            canvasX = Convert.ToInt32(myCanvas.Width);
            canvasY = Convert.ToInt32(myCanvas.Height);
            savedMap = new uint[canvasX * canvasY];
            myCanvas.Background = Brushes.Blue;
            copyMap = new uint[canvasX * canvasY]; ;

            //testing functions
            //DrawCheckers();
            //DrawLine(50, 50, 300, 300);
            //DrawLine(300, 50, 50, 300);

            //DrawTriangle(40, 40, Math.Min(myCanvas.Width, myCanvas.Height) - 90, WHITE);

            DrawMandleSet();


            Array.Copy(savedMap, copyMap, canvasX * canvasY);

            //DrawJuliaSet(a, b, limit, upBound);

            //DrawChaosTriangle(40, 40, Math.Min(canvasY, canvasX) - 90);
            Render();
        }

        private void btnStart_Click11(object sender, RoutedEventArgs e)
        {

            savedMap = new uint[canvasX * canvasY];
            if (isMandle)
            {
                DrawMandleSet();
            }
            else
            {
                DrawJuliaSet(a, b, limit, upBound, false);
            }
            Render();

        }



        private void myImage_MouseMove(object sender, MouseEventArgs e)
        {
            if (isStillDown)
            {
                //copy the copy to the saved map

                Array.Copy(copyMap, savedMap, canvasX * canvasY);



                //get curr mouse coordinates

                var position = e.GetPosition((IInputElement)sender);
                double currMouseX = position.X;
                double currMouseY = position.Y;

                if (currMouseY < canvasY)
                {

                    //draw a rectangle from mouse orig pos to new pos
                    //as a reminder I wrote the DrawLine function as a testing function that I decided to just re-use here
                    //and theres no need to convert it from pixelspace to userspace because id have to convert it back anyways
                    DrawLine(savedMouseX, savedMouseY, savedMouseX, currMouseY, BLACK);
                    DrawLine(currMouseX, savedMouseY, currMouseX, currMouseY, BLACK);
                    DrawLine(savedMouseX, currMouseY, currMouseX, currMouseY, BLACK);
                    DrawLine(savedMouseX, savedMouseY, currMouseX, savedMouseY, BLACK);
                    //render
                    Render();
                }


                //DrawCheckers();
            }
        }


        private void ImageMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (isStillDown)
            {
                isStillDown = false;
                //Array.Copy(copyMap, savedMap, canvasX * canvasY);//testing funct
                //get curr mouse position 
                var position = e.GetPosition((IInputElement)sender);
                double currMouseX = position.X;
                double currMouseY = position.Y;

                //check to see if both saved mouse pos and curr pos are valid
                if (currMouseX < savedMouseX)
                {
                    double temp = currMouseX;
                    currMouseX = savedMouseX;
                    savedMouseX = temp;
                }
                if (currMouseY < savedMouseY)
                {
                    double temp = currMouseY;
                    currMouseY = savedMouseY;
                    savedMouseY = temp;
                }

                if (savedMouseX >= 0 && currMouseX < canvasX && savedMouseY >= 0 && currMouseY < canvasY)
                {
                    //if take those coordinates and convert them to user coordinates relative to the current user coordinates
                    double newiXMin = (uXMax - uXMin) * (savedMouseX - myCanvas.Width) / myCanvas.Width + uXMax;
                    double newiYMin = (-1.0 * savedMouseY) / myCanvas.Height * (uYMax - uYMin) - uYMin;
                    double newiXMax = (uXMax - uXMin) * (currMouseX - myCanvas.Width) / myCanvas.Width + uXMax;
                    double newiYMax = (-1.0 * currMouseY) / myCanvas.Height * (uYMax - uYMin) - uYMin;

                    uXMin = newiXMin;
                    uXMax = newiXMax;
                    uYMin = newiYMin;
                    uYMax = newiYMax;


                    savedMap = new uint[canvasX * canvasY];

                    if (isMandle)
                    {
                        uYMax *= -1;
                        uYMin *= -1;
                        DrawMandleSet();
                    }
                    else
                    {


                        uXMax *= -1;
                        uXMin *= -1;
                        DrawJuliaSet(a, b, limit, upBound, false);
                    }
                }


                Render();
            }
        }
        //clock for zoom and shift click for selecting a julia from mandelbrot
        private void ImageMouseDown(object sender, MouseButtonEventArgs e)
        {
            //DrawCheckers();
            if ((Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)) && isMandle)
            {
                //get position
                var position = e.GetPosition((IInputElement)sender);
                double mouseX = (uXMax - uXMin) * (position.X - myCanvas.Width) / myCanvas.Width + uXMax;
                double mouseY = (-1.0 * position.Y) / myCanvas.Height * (uYMax - uYMin) - uYMin;

                //check if position contains a mandlebrot set
                double x1 = 0;
                double y1 = 0;

                double x2 = 0;//x2
                double y2 = 0;//y2
                double w = 0;
                int i = 0;
                for (; i < limit && x2 + y2 <= 4; i++)
                {
                    x1 = x2 - y2 + mouseX;
                    y1 = w - x2 - y2 + mouseY;
                    x2 = x1 * x1;
                    y2 = y1 * y1;
                    w = (x1 + y1) * (x1 + y1);

                }

                if (i >= limit)
                {
                    isMandle = false;
                    a = mouseX;
                    b = mouseY;
                    savedMap = new uint[canvasX * canvasY];
                    DrawJuliaSet(a, b, limit, upBound);
                }
            }

            else if (isMandle)
            {
                //save current mouse coordinates here


                var position = e.GetPosition((IInputElement)sender);
                savedMouseX = position.X;
                savedMouseY = position.Y;

                isStillDown = true;

                Array.Copy(savedMap, copyMap, canvasX * canvasY);


                //did this incorrectly due to a misunderstanding but because this code will be useful for reference im just commenting it out
                /*
                //retrieve position relative to image
                var position = e.GetPosition((IInputElement)sender);
                //calculate mouse userspace location
                double mouseX = (uXMax - uXMin) * (position.X - myCanvas.Width) / myCanvas.Width + uXMax;
                double mouseY = (-1.0 * position.Y) / myCanvas.Height * (uYMax - uYMin) - uYMin;
                //calculate new userspace mins and max
                double offsetY = (uYMax - uYMin) / zoomScaleRate / 2;
                uYMax = mouseY - offsetY;
                uYMin = mouseY + offsetY;
                uYMax *= -1;
                uYMin *= -1;
                double offsetX = (uXMax - uXMin) / zoomScaleRate / 2;
                uXMax = mouseX + offsetX;
                uXMin = mouseX - offsetX;
                savedMap = new uint[canvasX * canvasY];
                DrawMandleSet();
                */
            }

            else if (!isMandle && (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)))
            {

                ResetMandle();
            }
            //zoom on julia
            else if ((!Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift) && !isMandle))
            {
                var position = e.GetPosition((IInputElement)sender);
                savedMouseX = position.X;
                savedMouseY = position.Y;

                isStillDown = true;

                Array.Copy(savedMap, copyMap, canvasX * canvasY);
                /*
                var position = e.GetPosition((IInputElement)sender);
                double mouseX = (uXMax - uXMin) * (position.X - myCanvas.Width) / myCanvas.Width + uXMax;
                double mouseY = (-1.0 * position.Y) / myCanvas.Height * (uYMax - uYMin) - uYMin;

                double offsetY = (uYMax - uYMin) / zoomScaleRate / 2;
                uYMax = mouseY - offsetY;
                uYMin = mouseY + offsetY;

                double offsetX = (uXMax - uXMin) / zoomScaleRate / 2;
                uXMax = mouseX + offsetX;
                uXMin = mouseX - offsetX;


                uXMax *= -1;
                uXMin *= -1;

                savedMap = new uint[canvasX * canvasY];
                DrawJuliaSet(a, b, limit, upBound, false);
                */
            }

            //zoom on mandlebrot

            Render();
        }
        private void DrawJuliaSet(double a, double b, int limit, double upBound, bool reset = true)
        {
            TextBox2.Text = a.ToString();
            TextBox3.Text = b.ToString();
            if (reset)
            {
                uYMin = 2.0;
                uYMax = -2.0;
                uXMin = -2.0;
                uXMax = 2.0;
            }
            for (int x = 0; x < canvasX; x++)
            {
                for (int y = 0; y < canvasY; y++)
                {
                    //these two should be userspace
                    double prevX = ((uXMax - uXMin) * (x - canvasX)) / myCanvas.Width + uXMax;
                    double prevY = (y) / -myCanvas.Height * (uYMax - uYMin) - uYMin;
                    double xtemp;
                    double ytemp;
                    for (int i = 0; i < limit; i++)
                    {
                        xtemp = prevX * prevX - prevY * prevY + a;
                        ytemp = 2 * prevX * prevY + b;
                        prevX = xtemp;
                        prevY = ytemp;
                        if (prevX * prevX + prevY * prevY > upBound)
                        {
                            savedMap[canvasX * y + x] = RED + (uint)Math.Round(Convert.ToDouble(i) / Convert.ToDouble(limit) * 256);

                            i = limit;
                        }


                    }

                }
            }

        }
        private void Textbox1Changed(object sender, RoutedEventArgs e)
        {
            if (TextBox1.Text != "")
            {
                limit = Int32.Parse(TextBox1.Text);
            }
        }
        //julia buttons
        //yes I know it would have been better to just write a method rather than just copy paste it
        private void btnStart_Click1(object sender, RoutedEventArgs e)
        {
            a = -0.74543;
            b = 0.11301;
            isMandle = false;
            savedMap = new uint[canvasX * canvasY];
            DrawJuliaSet(a, b, limit, upBound);
            Render();
        }
        private void btnStart_Click2(object sender, RoutedEventArgs e)
        {
            a = 0.32;
            b = 0.043;
            isMandle = false;
            savedMap = new uint[canvasX * canvasY];
            DrawJuliaSet(a, b, limit, upBound);
            Render();
        }
        private void btnStart_Click3(object sender, RoutedEventArgs e)
        {
            a = 0.27334;
            b = 0.00742;
            isMandle = false;
            savedMap = new uint[canvasX * canvasY];
            DrawJuliaSet(a, b, limit, upBound);
            Render();
        }
        private void btnStart_Click4(object sender, RoutedEventArgs e)
        {
            a = -1.25;
            b = 0.11;
            isMandle = false;
            savedMap = new uint[canvasX * canvasY];
            DrawJuliaSet(a, b, limit, upBound);
            Render();
        }
        private void btnStart_Click5(object sender, RoutedEventArgs e)
        {
            a = 0.11031;
            b = -0.67037;
            isMandle = false;
            savedMap = new uint[canvasX * canvasY];
            DrawJuliaSet(a, b, limit, upBound);
            Render();
        }
        private void btnStart_Click6(object sender, RoutedEventArgs e)
        {
            a = 0;
            b = 1;
            isMandle = false;
            savedMap = new uint[canvasX * canvasY];
            DrawJuliaSet(a, b, limit, upBound);
            Render();
        }
        private void btnStart_Click7(object sender, RoutedEventArgs e)
        {
            a = -0.194;
            b = 0.6557;
            isMandle = false;
            savedMap = new uint[canvasX * canvasY];
            DrawJuliaSet(a, b, limit, upBound);
            Render();
        }
        private void btnStart_Click8(object sender, RoutedEventArgs e)
        {
            a = -0.15652;
            b = 1.03225;
            isMandle = false;
            savedMap = new uint[canvasX * canvasY];
            DrawJuliaSet(a, b, limit, upBound);
            Render();
        }
        private void btnStart_Click9(object sender, RoutedEventArgs e)
        {
            if (TextBox2.Text != "" && (TextBox3.Text != ""))
                a = Convert.ToDouble(TextBox2.Text);
            b = Convert.ToDouble(TextBox3.Text);
            isMandle = false;
            savedMap = new uint[canvasX * canvasY];
            DrawJuliaSet(a, b, limit, upBound);
            Render();
        }
        private void btnStart_Click10(object sender, RoutedEventArgs e)
        {
            ResetMandle();
            Render();
        }
        //testboxes for custom input
        private void TextBoxInput1(object sender, TextCompositionEventArgs e) //this I found while researching how to make a textbox that only accepts numbers in wpf
        {//apparently theres not many ways to do it without downloading a new package
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        private void TextBoxInput2(object sender, TextCompositionEventArgs e)
        {
        }
        private void TextBoxInput3(object sender, TextCompositionEventArgs e)
        {
        }
        private void ResetMandle() //function for resetting mandelbrot sets to a normal position
        {
            uYMin = -2.0;
            uYMax = 2.0;
            uXMin = -2.0;
            uXMax = 2.0;
            isMandle = true;
            savedMap = new uint[canvasX * canvasY];
            DrawMandleSet();
        }

        private void DrawMandleSet()//source for optimized algorithm is  https://en.wikipedia.org/wiki/Plotting_algorithms_for_the_Mandelbrot_set
        {
            for (int x = 0; x < canvasX; x++)
            {
                for (int y = 0; y < canvasY; y++)
                {
                    double x0 = ((uXMax - uXMin) * (x - canvasX)) / myCanvas.Width + uXMax;
                    double y0 = (y) / -myCanvas.Height * (uYMax - uYMin) - uYMin;//-2.0 - 1.0;
                    double x1 = 0;
                    double y1 = 0;
                    double x2 = 0;//x2
                    double y2 = 0;//y2
                    double w = 0;
                    int i = 0;
                    for (; i < limit && x2 + y2 <= 4; i++)
                    {
                        y1 = 2 * x1 * y1 + y0;
                        x1 = x2 - y2 + x0;
                        x2 = x1 * x1;
                        y2 = y1 * y1;
                    }
                    if (i < limit)
                    {
                        savedMap[canvasX * y + x] = RED;// + (uint)Math.Round(Convert.ToDouble(i) / Convert.ToDouble(limit) * 256);
                    }
                }
            }

        }

        private void Render()
        {
            WriteableBitmap bitmap = new WriteableBitmap(canvasX, canvasY, 96, 96, PixelFormats.Bgra32, null);


            bitmap.WritePixels(new Int32Rect(0, 0, canvasX, canvasY), savedMap, canvasX * 4, 0);
            myImage.Source = bitmap;
        }

        private void DrawLine(double x1, double y1, double x2, double y2, uint color = BLACK)//another testing function to prove my idea for the recursive triangle function
        {
            double x3 = (x1 + x2) / 2.0;
            double y3 = (y1 + y2) / 2.0;
            if ((int)Math.Round(x1) == (int)Math.Round(x3) && (int)Math.Round(y1) == (int)Math.Round(y3) || (int)Math.Round(x2) == (int)Math.Round(x3) && (int)Math.Round(y2) == (int)Math.Round(y3))
            {
                return;
            }
            else
            {
                if ((int)Math.Round(y3) < canvasY)
                {
                    savedMap[canvasX * (int)Math.Round(y3) + (int)Math.Round(x3)] = color;

                }
                DrawLine(x1, y1, x3, y3, color);
                DrawLine(x2, y2, x3, y3, color);
            }

        }

        private void DrawTriangle(double xOffset, double yOffset, double sideLength, uint color = WHITE)
        {
            double topX = xOffset + (sideLength / 2.0);
            double topY = yOffset;
            double leftX = xOffset;
            double rightX = xOffset + sideLength;
            double bottomY = yOffset + sideLength * Math.Sin(Math.PI / 3.0);

            RecursiveTriangle(leftX, bottomY, rightX, bottomY, topX, topY, color);
        }



        private void RecursiveTriangle(double x1, double y1, double x2, double y2, double x3, double y3, uint color = BLACK)
        {
            //x1 bottom left
            //x2 bottom right
            //x3 top center
            //x4 middle left
            //x5 middle right
            //x6 bottom center
            if ((int)Math.Round(x1) == (int)Math.Round(x3) && (int)Math.Round(y1) == (int)Math.Round(y3) || (int)Math.Round(x2) == (int)Math.Round(x3) && (int)Math.Round(y2) == (int)Math.Round(y3) || (int)Math.Round(x1) == (int)Math.Round(x2) && (int)Math.Round(y1) == (int)Math.Round(y2))
            {
                savedMap[canvasX * (int)Math.Round(y3) + (int)Math.Round(x3)] = color;
                return;
            }
            else
            {
                double x4 = (x1 + x3) / 2.0;
                double y4 = (y1 + y3) / 2.0;
                double x5 = (x2 + x3) / 2.0;
                double y5 = (y2 + y3) / 2.0;
                double x6 = (x1 + x2) / 2.0;
                double y6 = (y1 + y2) / 2.0;

                if (color == WHITE)
                {
                    RecursiveTriangle(x4, y4, x5, y5, x3, y3, RED);
                    RecursiveTriangle(x6, y6, x2, y2, x5, y5, GREEN);
                    RecursiveTriangle(x1, y1, x6, y6, x4, y4, BLUE);
                }
                else
                {
                    RecursiveTriangle(x4, y4, x5, y5, x3, y3, color);
                    RecursiveTriangle(x6, y6, x2, y2, x5, y5, color);
                    RecursiveTriangle(x1, y1, x6, y6, x4, y4, color);
                }
            }
        }

        private void DrawChaosTriangle(double xOffset, double yOffset, double sideLength)
        {
            double topX = xOffset + (sideLength / 2.0);
            double topY = yOffset;
            double leftX = xOffset;
            double rightX = xOffset + sideLength;
            double bottomY = yOffset + sideLength * Math.Sin(Math.PI / 3.0);

            double randomX;
            double randomY;

            //pick random coordinate
            Random rng = new Random();
            randomX = rng.NextDouble() * (rightX - leftX) + leftX;
            randomY = rng.NextDouble() * (bottomY - topY) + topY;

            for (int i = 0; i < 100000; i++)
            {
                //pick random point 0(left), 1(right), 2nn(top)
                int myPick = rng.Next(0, 3);

                //calculate midpoint
                if (myPick == 0)  //left
                {
                    randomX = (randomX + leftX) / 2.0;
                    randomY = (randomY + bottomY) / 2.0;
                }
                else if (myPick == 1)  //right
                {
                    randomX = (randomX + rightX) / 2.0;
                    randomY = (randomY + bottomY) / 2.0;
                }
                else
                {
                    randomX = (randomX + topX) / 2.0;
                    randomY = (randomY + topY) / 2.0;
                }
                //color pixel

                //check which point is closest
                double dist1 = Math.Sqrt((randomX - leftX) * (randomX - leftX) + (randomY - bottomY) * (randomY - bottomY));
                double dist2 = Math.Sqrt((randomX - rightX) * (randomX - rightX) + (randomY - bottomY) * (randomY - bottomY));
                double dist3 = Math.Sqrt((randomX - topX) * (randomX - topX) + (randomY - topY) * (randomY - topY));
                //savedMap[canvasX * (int)Math.Round(randomY) + (int)Math.Round(randomX)] = BLACK;
                if (dist1 < dist2 && dist1 < dist3)
                {
                    savedMap[canvasX * (int)Math.Round(randomY) + (int)Math.Round(randomX)] = BLUE;
                }
                else if (dist2 < dist1 && dist2 < dist3)
                {
                    savedMap[canvasX * (int)Math.Round(randomY) + (int)Math.Round(randomX)] = GREEN;
                }
                else
                {
                    savedMap[canvasX * (int)Math.Round(randomY) + (int)Math.Round(randomX)] = RED;
                }
            }

        }

        private void DrawCheckers()//this is just a testing function I made
        {

            WriteableBitmap bitmap = new WriteableBitmap(canvasX, canvasY, 96, 96, PixelFormats.Bgra32, null);
            // Create an array of pixels to contain pixel color values
            uint[] pixels = new uint[canvasX * canvasY];


            int checkersize = 50;
            for (int x = 0; x < canvasX; ++x)
            {
                for (int y = 0; y < canvasY; ++y)
                {
                    if (x / checkersize % 2 == (y / checkersize % 2))
                    {
                        int i = canvasX * y + x;
                        pixels[i] = WHITE;// uint.MaxValue;
                    }
                }
            }

            // apply pixels to bitmap
            bitmap.WritePixels(new Int32Rect(0, 0, canvasX, canvasY), pixels, canvasX * 4, 0);
            myImage.Source = bitmap;
        }


    }
}
