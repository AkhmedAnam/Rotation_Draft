using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using System.IO;

namespace RotationDraft
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private OpenFileDialog m_fileDialog;
        private string m_filePath;
        private BitmapSource m_srcImg;

        public string SelectedFile
        {
            get { return m_filePath; }

            set
            {
                m_filePath = value;

                try
                {
                    BitmapImage bmp = new BitmapImage(new Uri(m_filePath, UriKind.Absolute));
                    m_srcImg = bmp;
                    sourceImg.Source = bmp;
                }
                catch (FileNotFoundException e)
                {
                    MessageBox.Show("Ошибка при открытии файла. Информация: " + e.Message);
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            m_srcImg = sourceImg.Source as BitmapSource;
        }

        private async void angleValueSldr_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (m_srcImg != null)
            {
                double angle = e.NewValue;
                m_srcImg.Freeze();
                ImagePresenterInLayer firstPresenter = new ImagePresenterInLayer(m_srcImg.Clone(), 0),
                    secondPresenter = new ImagePresenterInLayer(m_srcImg.Clone(), 1);

                ImagesLayer layer = new ImagesLayer(m_srcImg.PixelHeight, m_srcImg.PixelWidth);

                layer.AddImagePresenter(firstPresenter); layer.AddImagePresenter(secondPresenter);

                progress.IsIndeterminate = true;
                textBox.IsEnabled = false; angleValueSldr.IsEnabled = false;

                rotatedImg.Source = await layer.GetResultRotatedImage(angle);

                progress.IsIndeterminate = false;
                textBox.IsEnabled = true; angleValueSldr.IsEnabled = true;

            }
        }

        private void loadImgBtn_Click(object sender, RoutedEventArgs e)
        {
            m_fileDialog = new OpenFileDialog();
            m_fileDialog.DefaultExt = ".bmp";
            m_fileDialog.Filter = "Image files |*.bmp;*.png;*.jpeg;*.jpg";
            m_fileDialog.FileOk += (send, ee) => this.SelectedFile = (send as OpenFileDialog).FileName;
            m_fileDialog.ShowDialog();

        }

        private void textBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var txt = (sender as TextBox).Text;
                txt = txt.Replace('.', ',');
                double val;

                if (Double.TryParse(txt, out val))
                    angleValueSldr.Value = val;
            }
        }
    }

    public class LayerPixelCenterPoint : IEquatable<LayerPixelCenterPoint>
    {
        public LayerPixelCenterPoint(Point point, int layerNumber, byte red, byte green, byte blue, double rotationAngleDegree = 0.0d)
        {
            PixelCenter = point; LayerNumber = layerNumber;
            RedColor = red; GreenColor = green; BlueColor = blue;
            RotationAngle = rotationAngleDegree;
        }

        static LayerPixelCenterPoint()
        {
            m_TopLeftCornerInOwnCoordinateSystem = new Point(-0.5d, 0.5d);
            m_TopRightCornerInOwnCoordinateSystem = new Point(0.5d, 0.5d);
            m_BottomLeftCornerInOwnCoordinateSystem = new Point(-0.5d, -0.5d);
            m_BottomRightCornerInOwnCoordinateSystem = new Point(0.5d, -0.5d);
        }

        public Point PixelCenter
        {
            get { return m_PixelCenter; }

            set
            {
                m_PixelCenter = new Point(value.X, value.Y);

                m_TopLeftCorner = new Point(m_PixelCenter.X + m_TopLeftCornerInOwnCoordinateSystem.X, m_PixelCenter.Y + m_TopLeftCornerInOwnCoordinateSystem.Y);
                m_TopRightCorner = new Point(m_PixelCenter.X + m_TopRightCornerInOwnCoordinateSystem.X, m_PixelCenter.Y + m_TopRightCornerInOwnCoordinateSystem.Y);
                m_BottomLeftCorner = new Point(m_PixelCenter.X + m_BottomLeftCornerInOwnCoordinateSystem.X, m_PixelCenter.Y + m_BottomLeftCornerInOwnCoordinateSystem.Y);
                m_BottomRightCorner = new Point(m_PixelCenter.X + m_BottomRightCornerInOwnCoordinateSystem.X, m_PixelCenter.Y + m_BottomRightCornerInOwnCoordinateSystem.Y);
            }
        }

        public double DistanceFromOrigin
        {
            get { return Math.Sqrt(Math.Pow(PixelCenter.X, 2) + Math.Pow(PixelCenter.Y, 2)); }
        }

        public double GetDistanceFromPoint(Point point)
        {
            return Math.Sqrt(Math.Pow(PixelCenter.X - point.X, 2) + Math.Pow(PixelCenter.Y - point.Y, 2));
        }

        public Tuple<Point, RelatedPixelSide>[] GetPointsBetweenSides(RelatedPixelSide arg1, RelatedPixelSide arg2)
        {
            if (arg1 == RelatedPixelSide.InsidePixel || arg2 == RelatedPixelSide.InsidePixel || arg1 == arg2)
                throw new ArgumentException("Arguments can not be 'Inside the pixel' and they can not be equal");
            else
            {
                if (Math.Abs(arg1 - arg2) == 2)//если рассматриваются стороны между которыми есть ещё одна сторона
                {
                    if (arg1 == RelatedPixelSide.FirstSide && arg2 == RelatedPixelSide.ThirdSide)
                        return new Tuple<Point, RelatedPixelSide>[] { new Tuple<Point, RelatedPixelSide>(this.TopRight, RelatedPixelSide.BetweenFirstAndSecondSide), new Tuple<Point, RelatedPixelSide>(this.BottomRight, RelatedPixelSide.BetweenFirstAndSecondSide) };
                    else if (arg1 == RelatedPixelSide.ThirdSide && arg2 == RelatedPixelSide.FirstSide)
                        return new Tuple<Point, RelatedPixelSide>[] { new Tuple<Point, RelatedPixelSide>(this.BottomLeft, RelatedPixelSide.BetweenThirdAndFourthSide), new Tuple<Point, RelatedPixelSide>(this.TopLeft, RelatedPixelSide.BetweenFourthAndFirstSide) };
                    else if (arg1 == RelatedPixelSide.SecondSide && arg2 == RelatedPixelSide.FourthSide)//первая сторона - 2ая, а вторая - 4ая
                        return new Tuple<Point, RelatedPixelSide>[] { new Tuple<Point, RelatedPixelSide>(this.BottomRight, RelatedPixelSide.BetweenFirstAndSecondSide), new Tuple<Point, RelatedPixelSide>(this.BottomLeft, RelatedPixelSide.BetweenThirdAndFourthSide) };
                    else//первая сторона - 4ая, а вторая - 2ая
                        return new Tuple<Point, RelatedPixelSide>[] { new Tuple<Point, RelatedPixelSide>(this.TopLeft, RelatedPixelSide.BetweenFourthAndFirstSide), new Tuple<Point, RelatedPixelSide>(this.TopRight, RelatedPixelSide.BetweenFirstAndSecondSide) };

                }
                else if (Math.Abs(arg1 - arg2) == 1)//рассматриваются близлежащие стороны (1ая и 2ая, 3ья и 4ая и тд)
                {
                    if ((arg1 == RelatedPixelSide.FirstSide && arg2 == RelatedPixelSide.SecondSide) || (arg1 == RelatedPixelSide.SecondSide && arg2 == RelatedPixelSide.FirstSide))
                        return new Tuple<Point, RelatedPixelSide>[] { new Tuple<Point, RelatedPixelSide>(this.TopRight, RelatedPixelSide.BetweenFirstAndSecondSide) };
                    else if ((arg1 == RelatedPixelSide.SecondSide && arg2 == RelatedPixelSide.ThirdSide) || (arg1 == RelatedPixelSide.ThirdSide && arg2 == RelatedPixelSide.SecondSide))
                        return new Tuple<Point, RelatedPixelSide>[] { new Tuple<Point, RelatedPixelSide>(this.BottomRight, RelatedPixelSide.BetweenFirstAndSecondSide) };
                    else if ((arg1 == RelatedPixelSide.ThirdSide && arg2 == RelatedPixelSide.FourthSide) || (arg1 == RelatedPixelSide.FourthSide && arg2 == RelatedPixelSide.ThirdSide))
                        return new Tuple<Point, RelatedPixelSide>[] { new Tuple<Point, RelatedPixelSide>(this.BottomLeft, RelatedPixelSide.BetweenThirdAndFourthSide) };
                    else
                        return new Tuple<Point, RelatedPixelSide>[] { new Tuple<Point, RelatedPixelSide>(this.TopLeft, RelatedPixelSide.BetweenFourthAndFirstSide) };
                }
                else
                    throw new ArgumentException("Error arguments");
            }

        }

        public bool IsPointBelongThisPixel(Point point)
        {
            bool toReturn = false;

            Point pointInOwnCS = new Point(point.X - this.PixelCenter.X, point.Y - this.PixelCenter.Y);

            if (this.RotationAngle != 0)
            {
                //LayerPixelCenterPoint lpcp = new LayerPixelCenterPoint(this.PixelCenter, this.LayerNumber, this.RedColor, this.GreenColor, this.BlueColor);

                Point rotatedPoint = RotatePoint(pointInOwnCS, -this.RotationAngle);

                toReturn = rotatedPoint.X > m_BottomLeftCornerInOwnCoordinateSystem.X && rotatedPoint.X < m_BottomRightCornerInOwnCoordinateSystem.X &&
                           rotatedPoint.Y > m_BottomLeftCornerInOwnCoordinateSystem.Y && rotatedPoint.Y < m_TopLeftCornerInOwnCoordinateSystem.Y;
            }
            else
                toReturn = pointInOwnCS.X > m_BottomLeftCornerInOwnCoordinateSystem.X && pointInOwnCS.X < m_BottomLeftCornerInOwnCoordinateSystem.X &&
                           pointInOwnCS.Y > m_BottomLeftCornerInOwnCoordinateSystem.Y && pointInOwnCS.Y < m_TopLeftCornerInOwnCoordinateSystem.Y;

            return toReturn;
        }

        public bool IsPointBelongPixelFirstSide(Point point)
        {
            if (RotationAngle == 0)
                return FirstSide.IsPointBelongToVector(point);
            else
                throw new NotImplementedException("LayerPixelCenterPoint.IsPointBelongPixelBoundaries method works onle for angle = 0.");
        }

        public bool IsPointBelongPixelSecondSide(Point point)
        {
            if (RotationAngle == 0)
                return SecondSide.IsPointBelongToVector(point);
            else
                throw new NotImplementedException("LayerPixelCenterPoint.IsPointBelongPixelSecondSide method works onle for angle = 0.");
        }

        public bool IsPointBelongPixelThirdSide(Point point)
        {
            if (RotationAngle == 0)
                return ThirdSide.IsPointBelongToVector(point);
            else
                throw new NotImplementedException("LayerPixelCenterPoint.IsPointBelongPixelThirdSide method works onle for angle = 0.");
        }

        public bool IsPointBelongPixelFourthSide(Point point)
        {
            if (RotationAngle == 0)
                return FourthSide.IsPointBelongToVector(point);
            else
                throw new NotImplementedException("LayerPixelCenterPoint.IsPointBelongPixelFourthSide method works onle for angle = 0.");
        }

        public static Point RotatePoint(Point point, double angleDegree)
        {
            Point toReturn = new Point(point.X, point.Y);

            double anglsInRad = angleDegree / 180.0d * Math.PI;

            if (anglsInRad > 0)//угол поворота больше 0 => новая система координат повёрнута против часовой
            {
                toReturn = new Point(point.X * Math.Cos(anglsInRad) - point.Y * Math.Sin(anglsInRad),
                                point.X * Math.Sin(anglsInRad) + point.Y * Math.Cos(anglsInRad));
            }
            else if (anglsInRad < 0)//по часовой
            {
                toReturn = new Point(point.X * Math.Cos(anglsInRad) + point.Y * Math.Sin(anglsInRad),
                                -point.X * Math.Sin(anglsInRad) + point.Y * Math.Cos(anglsInRad));
            }

            return toReturn;
        }

        public override bool Equals(object obj)
        {
            LayerPixelCenterPoint arg = obj as LayerPixelCenterPoint;

            if (arg != null)
                return Equals(arg);
            else
                return false;
        }

        public override int GetHashCode()
        {
            return this.RedColor + this.GreenColor + this.BlueColor + (int)(this.PixelCenter.X + 1) + (int)(this.PixelCenter.Y + 1);
        }

        public bool Equals(LayerPixelCenterPoint other)
        {
            Point roundBL = new Point(Math.Round(BottomLeft.X, 10), Math.Round(BottomLeft.Y, 10)), roundBL_Other = new Point(Math.Round(other.BottomLeft.X, 10), Math.Round(other.BottomLeft.Y, 10)),
                roundBR = new Point(Math.Round(BottomRight.X, 10), Math.Round(BottomRight.Y, 10)), roundBR_Other = new Point(Math.Round(other.BottomRight.X, 10), Math.Round(other.BottomRight.Y, 10)),
                roundPC = new Point(Math.Round(PixelCenter.X, 10), Math.Round(PixelCenter.Y, 10)), roundPC_Other = new Point(Math.Round(other.PixelCenter.X, 10), Math.Round(other.PixelCenter.Y, 10)),
                roundTL = new Point(Math.Round(TopLeft.X, 10), Math.Round(TopLeft.Y, 10)), roundTL_Other = new Point(Math.Round(other.TopLeft.X, 10), Math.Round(other.TopLeft.Y, 10)),
                roundTR = new Point(Math.Round(TopRight.X, 10), Math.Round(TopRight.Y, 10)), roundTR_Other = new Point(Math.Round(other.TopRight.X, 10), Math.Round(other.TopRight.Y, 10));


            return this.BlueColor == other.BlueColor &&
                       roundBL == roundBL_Other &&
                       roundBR == roundBR_Other &&
                       this.GreenColor == other.GreenColor &&
                       this.LayerNumber == other.LayerNumber &&
                       roundPC == roundPC_Other &&
                       this.RedColor == other.RedColor &&
                       this.RotationAngle == other.RotationAngle &&
                       roundTL == roundTL_Other &&
                       roundTR == roundTR_Other;
        }

        public int LayerNumber { get; private set; }

        public byte RedColor { get; private set; }

        public byte GreenColor { get; private set; }

        public byte BlueColor { get; private set; }

        public Point TopLeft { get { return m_TopLeftCorner; } }

        public Point TopRight { get { return m_TopRightCorner; } }

        public Point BottomLeft { get { return m_BottomLeftCorner; } }

        public Point BottomRight { get { return m_BottomRightCorner; } }

        /// <summary>
        /// From TopLeft point to TopRight point
        /// </summary>
        public FreeVector FirstSide { get { return new FreeVector(this.TopLeft, this.TopRight); } }

        /// <summary>
        /// From TopRight point to BottomRight point
        /// </summary>
        public FreeVector SecondSide { get { return new FreeVector(this.TopRight, this.BottomRight); } }

        /// <summary>
        /// From BottomRight point to BottomLeft point
        /// </summary>
        public FreeVector ThirdSide { get { return new FreeVector(this.BottomRight, this.BottomLeft); } }

        /// <summary>
        /// From BottomLeft point to TopLeft point
        /// </summary>
        public FreeVector FourthSide { get { return new FreeVector(this.BottomLeft, this.TopLeft); } }

        public double RotationAngle
        {
            get { return m_rotationAngleInDegree; }

            set
            {
                double anglsInRad = value / 180.0d * Math.PI;

                m_TopLeftCorner = new Point(PixelCenter.X + m_TopLeftCornerInOwnCoordinateSystem.X * Math.Cos(anglsInRad) - m_TopLeftCornerInOwnCoordinateSystem.Y * Math.Sin(anglsInRad),
                    PixelCenter.Y + m_TopLeftCornerInOwnCoordinateSystem.X * Math.Sin(anglsInRad) + m_TopLeftCornerInOwnCoordinateSystem.Y * Math.Cos(anglsInRad));

                m_TopRightCorner = new Point(PixelCenter.X + m_TopRightCornerInOwnCoordinateSystem.X * Math.Cos(anglsInRad) - m_TopRightCornerInOwnCoordinateSystem.Y * Math.Sin(anglsInRad),
                    PixelCenter.Y + m_TopRightCornerInOwnCoordinateSystem.X * Math.Sin(anglsInRad) + m_TopRightCornerInOwnCoordinateSystem.Y * Math.Cos(anglsInRad));

                m_BottomLeftCorner = new Point(PixelCenter.X + m_BottomLeftCornerInOwnCoordinateSystem.X * Math.Cos(anglsInRad) - m_BottomLeftCornerInOwnCoordinateSystem.Y * Math.Sin(anglsInRad),
                    PixelCenter.Y + m_BottomLeftCornerInOwnCoordinateSystem.X * Math.Sin(anglsInRad) + m_BottomLeftCornerInOwnCoordinateSystem.Y * Math.Cos(anglsInRad));

                m_BottomRightCorner = new Point(PixelCenter.X + m_BottomRightCornerInOwnCoordinateSystem.X * Math.Cos(anglsInRad) - m_BottomRightCornerInOwnCoordinateSystem.Y * Math.Sin(anglsInRad),
                    PixelCenter.Y + m_BottomRightCornerInOwnCoordinateSystem.X * Math.Sin(anglsInRad) + m_BottomRightCornerInOwnCoordinateSystem.Y * Math.Cos(anglsInRad));

                m_rotationAngleInDegree = value;
            }
        }

        public static bool operator ==(LayerPixelCenterPoint arg1, LayerPixelCenterPoint arg2)
        {
            return arg1.Equals(arg2);
        }

        public static bool operator !=(LayerPixelCenterPoint arg1, LayerPixelCenterPoint arg2)
        {
            return !arg1.Equals(arg2);
        }


        private double m_rotationAngleInDegree;
        private static readonly Point m_TopLeftCornerInOwnCoordinateSystem, m_TopRightCornerInOwnCoordinateSystem,
            m_BottomLeftCornerInOwnCoordinateSystem, m_BottomRightCornerInOwnCoordinateSystem;

        private Point m_TopLeftCorner, m_TopRightCorner, m_BottomLeftCorner, m_BottomRightCorner, m_PixelCenter;
    }

    public enum OriginOfImageCoordinateSystem
    {
        TopLeft,
        TopRight,
        Center,
        BottomLeft,
        BottomRight
    }

    public class ImagePresenterInLayer
    {
        public ImagePresenterInLayer(BitmapSource img, int layerNumber)
        {
            m_layerNumber = layerNumber;
            m_pixelCenters = new List<LayerPixelCenterPoint>();
            m_originOfCS = OriginOfImageCoordinateSystem.TopLeft;
            InitializeImage(img);
        }

        public OriginOfImageCoordinateSystem OriginOfCoordinateSystem
        {
            get { return m_originOfCS; }

            set
            {
                Point offset = GetOffsetFromOldOrigin(m_originOfCS, value);

                if (offset.X != 0 || offset.Y != 0)
                {
                    for (int i = 0; i < m_pixelCenters.Count; i++)
                    {
                        m_pixelCenters[i].PixelCenter = new Point(m_pixelCenters[i].PixelCenter.X - offset.X, m_pixelCenters[i].PixelCenter.Y - offset.Y);
                    }
                }

                m_originOfCS = value;
            }
        }

        public List<LayerPixelCenterPoint> PixelCenters
        {
            get
            {
                return m_pixelCenters;
            }
        }

        public unsafe WriteableBitmap WrBMP
        {
            get
            {
                WriteableBitmap wrBMP = new WriteableBitmap(m_imgPixWidth, m_imgPixHeight, m_dpiX, m_dpiY, m_pixFormat, null);

                int stride = wrBMP.BackBufferStride, bytesPerPix = wrBMP.Format.BitsPerPixel / 8;

                byte* imageData = (byte*)wrBMP.BackBuffer.ToPointer();

                for (int h = 0, posInPix = 0; h < m_imgPixHeight; h++, posInPix += stride - bytesPerPix * m_imgPixWidth)
                {
                    for (int w = 0; w < m_imgPixWidth; w++, posInPix += bytesPerPix)
                    {
                        //LayerPixelCenterPoint pointInLayer = new LayerPixelCenterPoint(currentPixelCenter, m_layerNumber, imageData[h * stride + w * bytesPerPix + 2],
                        //                                                imageData[h * stride + w * bytesPerPix + 1], imageData[h * stride + w * bytesPerPix]);
                        //m_pixelCenters.Add(pointInLayer);

                        imageData[h * stride + w * bytesPerPix] = m_pixelCenters[h * m_imgPixWidth + w].BlueColor;
                        imageData[h * stride + w * bytesPerPix + 1] = m_pixelCenters[h * m_imgPixWidth + w].GreenColor;
                        imageData[h * stride + w * bytesPerPix + 2] = m_pixelCenters[h * m_imgPixWidth + w].RedColor;

                    }
                }

                return wrBMP;

            }
        }

        public int ImgPixHeight
        {
            get
            {
                return m_imgPixHeight;
            }
        }

        public int ImgPixWidth
        {
            get
            {
                return m_imgPixWidth;
            }
        }

        //public ImagePixelVertexPresenter ImageVertexes
        //{
        //    get { return new ImagePixelVertexPresenter(this); }
        //}

        public LayerPixelCenterPoint[] GetPixelsWithCondition(Predicate<LayerPixelCenterPoint> condition)
        {
            var toReturn = from i in m_pixelCenters
                           where condition(i)
                           select i;

            return toReturn.ToArray();
        }

        public void TransferPixelCenters(Point offset)
        {
            for (int i = 0; i < m_pixelCenters.Count; i++)
            {
                m_pixelCenters[i].PixelCenter = new Point(m_pixelCenters[i].PixelCenter.X + offset.X, m_pixelCenters[i].PixelCenter.Y + offset.Y);
            }

        }

        public void RotateImage(double angleDegree)
        {
            double angleInRad = angleDegree / 180 * Math.PI;

            for (int i = 0; i < m_pixelCenters.Count; i++)
            {
                Point pixCentPoint = m_pixelCenters[i].PixelCenter, updatedPixCenterPoint = new Point();

                updatedPixCenterPoint.X = pixCentPoint.X * Math.Cos(angleInRad) - pixCentPoint.Y * Math.Sin(angleInRad);
                updatedPixCenterPoint.Y = pixCentPoint.X * Math.Sin(angleInRad) + pixCentPoint.Y * Math.Cos(angleInRad);

                m_pixelCenters[i].PixelCenter = updatedPixCenterPoint;
                m_pixelCenters[i].RotationAngle = angleDegree;
            }
        }

        private unsafe void InitializeImage(BitmapSource img)
        {
            WriteableBitmap wrBMP = new WriteableBitmap(img);
            Point currentPixelCenter = new Point(0.5d, -0.5d);//координаты первого пикселя, с которого начинается инициализация (левый верхний угол). Спускаясь вниз по строкам пикселей координата Y уменьшается

            int pixWidth = wrBMP.PixelWidth, pixHeight = wrBMP.PixelHeight, stride = wrBMP.BackBufferStride, bytesPerPix = wrBMP.Format.BitsPerPixel / 8;
            m_imgPixHeight = pixHeight; m_imgPixWidth = pixWidth; m_dpiX = wrBMP.DpiX; m_dpiY = wrBMP.DpiY; m_pixFormat = wrBMP.Format;

            byte* imageData = (byte*)wrBMP.BackBuffer.ToPointer();

            for (int h = 0, posInPix = 0; h < pixHeight; h++, posInPix += stride - bytesPerPix * pixWidth)
            {
                for (int w = 0; w < pixWidth; w++, posInPix += bytesPerPix)
                {
                    LayerPixelCenterPoint pointInLayer = new LayerPixelCenterPoint(currentPixelCenter, m_layerNumber, imageData[h * stride + w * bytesPerPix + 2],
                                                                    imageData[h * stride + w * bytesPerPix + 1], imageData[h * stride + w * bytesPerPix]);
                    m_pixelCenters.Add(pointInLayer);

                    currentPixelCenter.Offset(1, 0);
                }

                currentPixelCenter.Offset(0, -1);
                currentPixelCenter.X = 0.5d;
            }
        }
        private Point GetOffsetFromOldOrigin(OriginOfImageCoordinateSystem oldOrigin, OriginOfImageCoordinateSystem newOrigin)
        {
            Point toReturn = new Point();

            if (oldOrigin == OriginOfImageCoordinateSystem.TopLeft && newOrigin == OriginOfImageCoordinateSystem.Center)
            {
                toReturn.X = (double)m_imgPixWidth / 2.0d;
                toReturn.Y = -(double)m_imgPixHeight / 2.0d;
            }
            else if (oldOrigin == OriginOfImageCoordinateSystem.Center && newOrigin == OriginOfImageCoordinateSystem.TopLeft)
            {
                toReturn.X = -(double)m_imgPixWidth / 2.0d;
                toReturn.Y = (double)m_imgPixHeight / 2.0d;
            }
            else if (oldOrigin == newOrigin)
            {
                toReturn.X = 0;
                toReturn.Y = 0;
            }
            else
                throw new NotImplementedException("Метод 'GetOffsetFromOldOrigin' рассматривает переходы начала системы координат только от левого-верхнего угла в центр и наоборот");

            return toReturn;
        }


        private List<LayerPixelCenterPoint> m_pixelCenters;
        private OriginOfImageCoordinateSystem m_originOfCS;
        private int m_imgPixHeight, m_imgPixWidth, m_layerNumber;
        private double m_dpiX, m_dpiY;
        private PixelFormat m_pixFormat;
    }

    public class ImagesLayer
    {
        public ImagesLayer(int imgsPixelHeight, int imgsPixelWidth)
        {
            m_layersCounter = 0;
            m_allImages = new List<ImagePresenterInLayer>();
        }

        public ImagesLayer(BitmapSource srcImg) : this(srcImg.PixelHeight, srcImg.PixelWidth) { }

        public void AddImagePresenter(ImagePresenterInLayer toAdd)
        {
            m_allImages.Add(toAdd);
            m_layersCounter++;
        }

        public Task<WriteableBitmap> GetResultRotatedImage(double angleInDegree)
        {
            return Task<WriteableBitmap>.Run(() =>
            {

                m_allImages.ForEach((imgPres) => imgPres.OriginOfCoordinateSystem = OriginOfImageCoordinateSystem.Center);
                m_allImages[1].RotateImage(angleInDegree);
                List<LayerPixelCenterPoint> initialImgCenterPoints = m_allImages[0].PixelCenters;

                for (int i = 0; i < initialImgCenterPoints.Count; i++)
                {
                    LayerPixelCenterPoint[] cmposedPixels = m_allImages[1].GetPixelsWithCondition((pix) => pix.GetDistanceFromPoint(initialImgCenterPoints[i].PixelCenter) < 1.5d);//root(2) = 1.5

                    initialImgCenterPoints[i] = GetNewPixelCenterPoint(initialImgCenterPoints[i], cmposedPixels);
                }

                var toRet = m_allImages[0].WrBMP;
                toRet.Freeze();

                return toRet;
            });
        }

        /// <summary>
        /// Возвращает новое представление пикселя: пиксель результирующего повёрнутого изображения
        /// </summary>
        /// <param name="old">
        /// Старое представление неповёрнутого пикселя.
        /// </param>
        /// <param name="composedPixels">
        /// Массив повёрнутых пикселей, которые пересекают целевой неповёрнутый пиксель
        /// </param>
        /// <returns></returns>
        public LayerPixelCenterPoint GetNewPixelCenterPoint(LayerPixelCenterPoint old, params LayerPixelCenterPoint[] composedPixels)//ВЕКТОРА ВЕРТЕКСОВ НУЖНО РАССМАТРИВАТЬ ПОПАРНО, Т.К. ЦВЕТ ПИКСЕЛЯ ПОВЁРНУТОГО ИЗОБРАЖЕНИЯ ОПРЕДЕЛЯЕТСЯ ДВУМЯ ВЕКТОРАМИ ВЕРТЕКСА
        {
            LayerPixelCenterPoint toReturn = null;

            if (composedPixels.Length > 1)
            {
                List<Piece> pieceList = new List<Piece>();
                Piece piece = new Piece();

                FreeVector topDivider = new FreeVector(new Point(old.TopLeft.X - 10, old.TopLeft.Y), new Point(old.TopRight.X + 10, old.TopRight.Y)),
                rightDivider = new FreeVector(new Point(old.TopRight.X, old.TopRight.Y - 10), new Point(old.BottomRight.X, old.BottomRight.Y + 10)),
                bottomDivider = new FreeVector(new Point(old.BottomLeft.X - 10, old.BottomLeft.Y), new Point(old.BottomRight.X + 10, old.BottomRight.Y)),
                leftDivider = new FreeVector(new Point(old.TopLeft.X, old.TopLeft.Y - 10), new Point(old.BottomLeft.X, old.BottomLeft.Y + 10));

                for (int i = 0; i < composedPixels.Length; i++)
                {
                    piece = new Piece(Color.FromRgb(composedPixels[i].RedColor, composedPixels[i].GreenColor, composedPixels[i].BlueColor));

                    piece.FigureVertexes.AddRange(new PieceVertex[] {
                new PieceVertex(composedPixels[i].TopLeft),
                new PieceVertex(composedPixels[i].TopRight),
                new PieceVertex(composedPixels[i].BottomRight),
                new PieceVertex(composedPixels[i].BottomLeft)});

                    piece = Piece.TrimTop(piece, topDivider);
                    piece = Piece.TrimRight(piece, rightDivider);
                    piece = Piece.TrimBottom(piece, bottomDivider);
                    piece = Piece.TrimLeft(piece, leftDivider);

                    pieceList.Add(piece);

                }

                byte red = 0, green = 0, blue = 0;

                foreach (Piece p in pieceList)
                {
                    try
                    {
                        double koeff = p.Area;//Площадь целого пикселя = 1 => площадь осколка и естьь весовой коэффициент (сумма площадей всех осколков, входящих в пиксель = 1)

                        red += (byte)(koeff * p.PieceColor.R);
                        green += (byte)(koeff * p.PieceColor.G);
                        blue += (byte)(koeff * p.PieceColor.B);
                    }
                    catch (Exception) { }
                }

                toReturn = new LayerPixelCenterPoint(old.PixelCenter, old.LayerNumber, red, green, blue, old.RotationAngle);
            }
            else if (composedPixels.Length == 1)
            {
                toReturn = new LayerPixelCenterPoint(composedPixels[0].PixelCenter, 0, composedPixels[0].RedColor, composedPixels[0].GreenColor, composedPixels[0].BlueColor);
            }
            else
                toReturn = new LayerPixelCenterPoint(old.PixelCenter, old.LayerNumber, 255, 255, 255);

            return toReturn;
        }


        private int m_layersCounter;
        private List<ImagePresenterInLayer> m_allImages;
    }


    public static class VectorsProcessing//https://habrahabr.ru/post/267037/ - алгоритм определения пересечения отрезков и нахождения точки пересечения
    {
        /// <summary>
        /// Метод определяет пересекаются ли данные два вектораю
        /// </summary>
        /// <param name="v1">Первый свободный вектор</param>
        /// <param name="v2">Второй свободный вектор</param>
        /// <returns></returns>
        public static bool IsVectorsIntersect(FreeVector v1, FreeVector v2)
        {
            bool toReturn = false;

            bool startPointCondition = false, endPointCondition = false;

            Vector temp1 = new Vector(v1.StartPoint.X - v2.StartPoint.X, v1.StartPoint.Y - v2.StartPoint.Y),
                temp2 = new Vector(v1.EndPoint.X - v2.StartPoint.X, v1.EndPoint.Y - v2.StartPoint.Y),
                mainVector = new Vector(v2.EndPoint.X - v2.StartPoint.X, v2.EndPoint.Y - v2.StartPoint.Y);

            double temp1MainRes = Vector.CrossProduct(mainVector, temp1), temp2MainRes = Vector.CrossProduct(mainVector, temp2);
            startPointCondition = temp1MainRes * temp2MainRes < 0;

            mainVector = new Vector(v1.EndPoint.X - v1.StartPoint.X, v1.EndPoint.Y - v1.StartPoint.Y);
            temp1 = new Vector(v2.StartPoint.X - v1.StartPoint.X, v2.StartPoint.Y - v1.StartPoint.Y);
            temp2 = new Vector(v2.EndPoint.X - v1.StartPoint.X, v2.EndPoint.Y - v1.StartPoint.Y);

            temp1MainRes = Vector.CrossProduct(mainVector, temp1); temp2MainRes = Vector.CrossProduct(mainVector, temp2);

            endPointCondition = temp1MainRes * temp2MainRes < 0;

            toReturn = startPointCondition && endPointCondition;

            return toReturn;
        }

        /// <summary>
        /// Метод возвращает точку пересечения двух свободных векторов.
        /// Результат имеет смысл если метода IsVectorsIntersect вернул значение True.
        /// </summary>
        /// <param name="v1">Первый свободный вектор</param>
        /// <param name="v2">Второй свободный вектор</param>
        /// <returns></returns>
        public static Point GetIntersectionPoint(FreeVector v1, FreeVector v2)
        {
            Vector AC = new Vector(v2.StartPoint.X - v1.StartPoint.X, v2.StartPoint.Y - v1.StartPoint.Y),
                AD = new Vector(v2.EndPoint.X - v1.StartPoint.X, v2.EndPoint.Y - v1.StartPoint.Y),
                AB = new Vector(v1.EndPoint.X - v1.StartPoint.X, v1.EndPoint.Y - v1.StartPoint.Y);

            double xCoor = v2.StartPoint.X + (v2.EndPoint.X - v2.StartPoint.X)
                * (Math.Abs(Vector.CrossProduct(AB, AC)) / Math.Abs(Vector.CrossProduct(AB, AD) - Vector.CrossProduct(AB, AC))),

                yCoor = v2.StartPoint.Y + (v2.EndPoint.Y - v2.StartPoint.Y)
                * (Math.Abs(Vector.CrossProduct(AB, AC)) / Math.Abs(Vector.CrossProduct(AB, AD) - Vector.CrossProduct(AB, AC)));

            return new Point(xCoor, yCoor);
        }

        /// <summary>
        /// Метод определяет можно ли из данного набора векторов построить замкнутую фигуру.
        /// </summary>
        /// <param name="freeVectors">Массив рассматриваемых векторов</param>
        /// <returns></returns>
        public static bool AreVectorsFormClosedFigure(params FreeVector[] freeVectors)
        {
            bool isPointsOnLine = true;

            if (freeVectors.Length > 1)
            {
                for (int i = 1; i < freeVectors.Length; i++)
                {
                    //toReturn = toReturn || ((freeVectors[i].StartPoint.X != freeVectors[i].EndPoint.X) || (freeVectors[i + 1].StartPoint.X != freeVectors[i + 1].EndPoint.X))
                    //    || ((freeVectors[i].StartPoint.Y != freeVectors[i].EndPoint.Y) || (freeVectors[i + 1].StartPoint.Y != freeVectors[i + 1].EndPoint.Y));

                    //если координаты начала данного вектора (и х и у) отличаются от координат конца следующего вектора (и ч и у) то из этих векторов можно построить замкнутую фигуру
                    //toReturn = toReturn || ((freeVectors[i].StartPoint.X != freeVectors[i + 1].EndPoint.X) && (freeVectors[i].StartPoint.Y != freeVectors[i + 1].EndPoint.Y));

                    isPointsOnLine = isPointsOnLine && IsPointBelongsToLine(freeVectors[0].StartPoint, freeVectors[0].EndPoint, freeVectors[i].StartPoint)
                                     && IsPointBelongsToLine(freeVectors[0].StartPoint, freeVectors[0].EndPoint, freeVectors[i].EndPoint);

                    if (!isPointsOnLine)
                        break;
                }
            }

            return !isPointsOnLine;
        }

        public static double CalculateAreaOfClosedFigure(IEnumerable<FreeVector> freeVectors)
        {
            double toReturn = 0.0d;

            if (freeVectors.Count() > 1 && freeVectors.First().StartPoint == freeVectors.Last().EndPoint)
            {
                Point firstP = freeVectors.First().StartPoint, lastP = freeVectors.Last().StartPoint,
                    pointInside = new Point((firstP.X + lastP.X) / 2.0d, (firstP.Y + lastP.Y) / 2.0d);

                Vector v1 = new Vector(), v2 = new Vector();
                for (int i = 0; i < freeVectors.Count(); i++)
                {
                    v1.X = freeVectors.ElementAt(i).StartPoint.X - pointInside.X;
                    v1.Y = freeVectors.ElementAt(i).StartPoint.Y - pointInside.Y;

                    v2.X = freeVectors.ElementAt(i).EndPoint.X - pointInside.X;
                    v2.Y = freeVectors.ElementAt(i).EndPoint.Y - pointInside.Y;

                    toReturn += 0.5d * Math.Abs(Vector.CrossProduct(v1, v2)); //вычисляется площадь одного треугольника, входящего в состав осколка
                }


            }

            return toReturn;
        }

        private static bool IsPointBelongsToLine(Point firstLinePoint, Point secondLinePoint, Point point)
        {
            bool toReturn = false;

            if (firstLinePoint.X == secondLinePoint.X)
            {
                toReturn = point.X == firstLinePoint.X;
            }
            else if (firstLinePoint.Y == secondLinePoint.Y)
            {
                toReturn = point.Y == firstLinePoint.Y;
            }
            else
            {
                toReturn = (point.Y - firstLinePoint.Y) / (secondLinePoint.Y - firstLinePoint.Y)
                     ==
                       (point.X - firstLinePoint.X) / (secondLinePoint.X - firstLinePoint.X);
            }

            return toReturn;
        }

    }

    public struct FreeVector
    {
        public FreeVector(Point startPoint, Point endPoint)
        {
            m_startPoint = startPoint; m_endPoint = endPoint;
        }



        public Point StartPoint
        {
            get
            {
                return m_startPoint;
            }

            set
            {
                m_startPoint = value;
            }
        }

        public Point EndPoint
        {
            get
            {
                return m_endPoint;
            }

            set
            {
                m_endPoint = value;
            }
        }

        public bool IsPointBelongToVector(Point point)
        {
            return (point.Y - StartPoint.Y) / (EndPoint.Y - StartPoint.Y) == (point.X - StartPoint.X) / (EndPoint.X - StartPoint.X);
        }

        public override bool Equals(object obj)
        {

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return (int)(this.StartPoint.X + this.StartPoint.Y + this.EndPoint.X + this.EndPoint.Y);
        }

        public static bool operator ==(FreeVector v1, FreeVector v2)
        {
            return (v1.StartPoint == v2.StartPoint) && (v1.EndPoint == v2.EndPoint);
        }

        public static bool operator !=(FreeVector v1, FreeVector v2)
        {
            return !(v1 == v2);
        }


        public static FreeVector NULLVECTOR { get { return new FreeVector(new Point(0, 0), new Point(0, 0)); } }

        private Point m_startPoint, m_endPoint;
    }

    /// <summary>
    /// Перечисление, описывающее принадлежность вершины осколка к определённой стороне пикселя
    /// Нумераций сторон пикселя идёт по часовой стрелке, начиная с верхнего ребра (грани)
    /// </summary>
    [Flags]
    public enum RelatedPixelSide
    {
        NotImpotant = -1,
        InsidePixel = 0x0,//Вершина внутри пикселя
        FirstSide = 0x1,//Вершина лежит на первой (верхней) грани пикселя
        SecondSide = 0x2,//Вершина лежит на второй (правой) грани пикселя
        ThirdSide = 0x4,//Вершина лежит на третьей (нижней) грани пикселя
        FourthSide = 0x8,//Вершина лежит на четвёртой (левой) грани пикселя
        BetweenFirstAndSecondSide = 0x3,//между 1ой и 2ой стороной (TopRight вершина пикеля)
        BetweenSecondAndThirdSide = 0x6,//между 2ой и 3ьей стороной (BottomRight вершина пикеля)
        BetweenThirdAndFourthSide = 0xC,//между 3ьей и 4ой стороной (BottomLeft вершина пикеля)
        BetweenFourthAndFirstSide = 0x9//между 4ой и 1ой стороной (TopLeft вершина пикеля)
    }

    /// <summary>
    /// Структура, описывающая вершину осколка пикселя
    /// </summary>
    public struct PieceVertex : IEquatable<PieceVertex>
    {

        public PieceVertex(Point point, Point beforeNeighbor, Point nextNeighbor, RelatedPixelSide pixelSide) : this()
        {
            this.Point = point; this.PixelSide = pixelSide;
            BeforeNeighbor = beforeNeighbor; NextNeighbor = nextNeighbor;
        }

        public PieceVertex(Point point) : this()
        {
            this.Point = point; this.PixelSide = RelatedPixelSide.NotImpotant;
            BeforeNeighbor = point; NextNeighbor = point;
        }

        public PieceVertex(Point point, RelatedPixelSide side) : this()
        {
            this.Point = point; this.PixelSide = side;
            BeforeNeighbor = point; NextNeighbor = point;
        }

        public Point Point { get; set; }

        public Point BeforeNeighbor { get; set; }

        public FreeVector ToBeforeNeighborVector { get { return new FreeVector(this.Point, this.BeforeNeighbor); } }

        public Point NextNeighbor { get; set; }

        public FreeVector ToNextNeighborVector { get { return new FreeVector(this.Point, this.NextNeighbor); } }

        public RelatedPixelSide PixelSide { get; set; }

        public bool Equals(PieceVertex other)
        {
            return Point == other.Point;
        }
    }

    /// <summary>
    /// Структура, описывающая осколок пикселя
    /// </summary>
    public struct Piece
    {
        public Piece(Color color) : this()
        {
            PieceColor = color; this.FigureVertexes = new List<PieceVertex>();
        }

        public List<PieceVertex> FigureVertexes { get; set; }

        public Color PieceColor { get; set; }

        public PieceVertex[] GetVertexesWithCondition(Predicate<PieceVertex> condition)
        {
            return (from v in FigureVertexes
                    where condition(v)
                    select v).ToArray();
        }

        public override bool Equals(object obj)
        {
            bool toReturn = true;

            try
            {
                Piece other = (Piece)obj;
                other.FigureVertexes.Sort((v1, v2) =>
                {
                    double Diff = v1.Point.X - v2.Point.X;
                    if (Diff > 0)
                        return 1;
                    else if (Diff < 0)
                        return -1;
                    else
                    {
                        Diff = v1.Point.Y - v2.Point.Y;
                        return (int)Diff;
                    }
                });//сортируем по Х
                FigureVertexes.Sort((v1, v2) =>
                {
                    double Diff = v1.Point.X - v2.Point.X;
                    if (Diff > 0)
                        return 1;
                    else if (Diff < 0)
                        return -1;
                    else
                    {
                        Diff = v1.Point.Y - v2.Point.Y;
                        return (int)Diff;
                    }
                });

                for (int i = 0; i < FigureVertexes.Count; i++)
                {
                    toReturn = toReturn && FigureVertexes[i].Equals(other.FigureVertexes[i]);
                    if (!toReturn)
                        break;
                }

                toReturn = toReturn && PieceColor == other.PieceColor;
            }
            catch (Exception) { toReturn = false; }

            return toReturn;
        }

        public override int GetHashCode()
        {
            int toReturn = 0;

            foreach (var v in FigureVertexes)
            {
                toReturn += (int)(v.Point.X + v.Point.Y);
            }

            return toReturn;
        }

        public static bool operator ==(Piece p1, Piece p2)
        {
            bool toRet = true;
            p1.FigureVertexes.Sort((v1, v2) =>
            {
                double Diff = v1.Point.X - v2.Point.X;
                if (Diff > 0)
                    return 1;
                else if (Diff < 0)
                    return -1;
                else
                {
                    Diff = v1.Point.Y - v2.Point.Y;
                    return (int)Diff;
                }
            });
            p2.FigureVertexes.Sort((v1, v2) =>
            {
                double Diff = v1.Point.X - v2.Point.X;
                if (Diff > 0)
                    return 1;
                else if (Diff < 0)
                    return -1;
                else
                {
                    Diff = v1.Point.Y - v2.Point.Y;
                    return (int)Diff;
                }
            });

            try
            {
                for (int i = 0; i < p1.FigureVertexes.Count; i++)
                {
                    toRet = toRet && p1.FigureVertexes[i].Equals(p2.FigureVertexes[i]);
                    if (!toRet)
                        break;
                }

                toRet = toRet && p1.PieceColor == p2.PieceColor;
            }
            catch { toRet = false; }

            return toRet;
        }

        public static bool operator !=(Piece p1, Piece p2)
        {
            return !(p1 == p2);
        }

        public double Area
        {
            get
            {
                List<FreeVector> vectors = new List<FreeVector>();
                FreeVector current = new FreeVector();

                int i = 0;
                for (; i < FigureVertexes.Count - 1; i++)
                {
                    current.StartPoint = FigureVertexes[i].Point;
                    current.EndPoint = FigureVertexes[i + 1].Point;

                    vectors.Add(current);
                }

                current.StartPoint = FigureVertexes[i].Point;
                current.EndPoint = FigureVertexes[0].Point;

                vectors.Add(current);

                if (!VectorsProcessing.AreVectorsFormClosedFigure(vectors.ToArray()))
                    throw new FigureNotClosedException("Error while calculating area of a piece: figure of the piece is not closed");
                else
                    return VectorsProcessing.CalculateAreaOfClosedFigure(vectors);
            }
        }

        /// <summary>
        /// Метод для обрезания осколка (многоугольника) верхней полуплоскостью квадрата.
        /// </summary>
        /// <param name="old">Исходный осколок</param>
        /// <param name="topDivider">Вектор - нижняя граница верхней полуплоскости</param>
        public static Piece TrimTop(Piece old, FreeVector topDivider)
        {
            Piece toReturn = new Piece(old.PieceColor);

            var oldVertexes = old.FigureVertexes;

            if (oldVertexes.Count > 0)
            {
                List<Point> _newVertexes = new List<Point>();

                if (oldVertexes[0].Point.Y <= topDivider.StartPoint.Y)//если начальная точка лежит в нужной полуплоскости
                    _newVertexes.Add(oldVertexes[0].Point);

                FreeVector currentSide = new FreeVector();

                for (int i = 1; i < oldVertexes.Count; i++)
                {
                    currentSide = new FreeVector(oldVertexes[i - 1].Point, oldVertexes[i].Point);

                    if (VectorsProcessing.IsVectorsIntersect(currentSide, topDivider))
                    {
                        Point intersP = VectorsProcessing.GetIntersectionPoint(currentSide, topDivider);
                        _newVertexes.Add(intersP);

                        if (oldVertexes[i].Point.Y < topDivider.StartPoint.Y)//если вектор входит в нужную часть полуплоскости
                            _newVertexes.Add(oldVertexes[i].Point);
                    }
                    else if (oldVertexes[i].Point.Y <= topDivider.StartPoint.Y)//если вектор currentSide (текущая грань) Целиком лежит в нужной части полуплоскости
                    {
                        _newVertexes.Add(oldVertexes[i].Point);
                    }
                }

                //рассматриваем последнюю грань - вектор от последней точки многоугольника к первой.
                //Т.к. первая точка уже проверялась, то проверяем только пересечение
                currentSide = new FreeVector(oldVertexes.Last().Point, oldVertexes.First().Point);

                if (VectorsProcessing.IsVectorsIntersect(currentSide, topDivider))
                {
                    Point intersP = VectorsProcessing.GetIntersectionPoint(currentSide, topDivider);
                    _newVertexes.Add(intersP);
                }

                toReturn.FigureVertexes = (from p in _newVertexes select new PieceVertex(p)).ToList();
            }

            return toReturn;
        }

        /// <summary>
        /// Метод для обрезания осколка (многоугольника) правой полуплоскостью квадрата.
        /// </summary>
        /// <param name="old">Исходный осколок</param>
        /// <param name="rightDivider">Вектор - левая граница правой полуплоскости</param>
        public static Piece TrimRight(Piece old, FreeVector rightDivider)
        {
            Piece toReturn = new Piece(old.PieceColor);

            var oldVertexes = old.FigureVertexes;

            if (oldVertexes.Count > 0)
            {
                List<Point> _newVertexes = new List<Point>();

                if (oldVertexes[0].Point.X <= rightDivider.StartPoint.X)
                    _newVertexes.Add(oldVertexes[0].Point);

                FreeVector currentSide = new FreeVector();

                for (int i = 1; i < oldVertexes.Count; i++)
                {
                    currentSide = new FreeVector(oldVertexes[i - 1].Point, oldVertexes[i].Point);

                    if (VectorsProcessing.IsVectorsIntersect(currentSide, rightDivider))
                    {
                        Point intersP = VectorsProcessing.GetIntersectionPoint(currentSide, rightDivider);
                        _newVertexes.Add(intersP);

                        if (oldVertexes[i].Point.X < rightDivider.StartPoint.X)//если вектор входит в нужную часть полуплоскости
                            _newVertexes.Add(oldVertexes[i].Point);

                    }
                    else if (oldVertexes[i].Point.X <= rightDivider.StartPoint.X)//если вектор currentSide (текущая грань) Целиком лежит в нужной части полуплоскости
                    {
                        _newVertexes.Add(oldVertexes[i].Point);
                    }
                }

                //рассматриваем последнюю грань - вектор от последней точки многоугольника к первой.
                //Т.к. первая точка уже проверялась, то проверяем только пересечение
                currentSide = new FreeVector(oldVertexes.Last().Point, oldVertexes.First().Point);

                if (VectorsProcessing.IsVectorsIntersect(currentSide, rightDivider))
                {
                    Point intersP = VectorsProcessing.GetIntersectionPoint(currentSide, rightDivider);
                    _newVertexes.Add(intersP);
                }

                toReturn.FigureVertexes = (from p in _newVertexes select new PieceVertex(p)).ToList();
            }

            return toReturn;
        }

        /// <summary>
        /// Метод для обрезания осколка (многоугольника) правой полуплоскостью квадрата.
        /// </summary>
        /// <param name="old">Исходный осколок</param>
        /// <param name="bottomDivider">Вектор - верхняя граница нижней полуплоскости</param>
        public static Piece TrimBottom(Piece old, FreeVector bottomDivider)
        {
            Piece toReturn = new Piece(old.PieceColor);

            var oldVertexes = old.FigureVertexes;

            if (oldVertexes.Count > 0)
            {
                List<Point> _newVertexes = new List<Point>();

                if (oldVertexes[0].Point.Y >= bottomDivider.StartPoint.Y)
                    _newVertexes.Add(oldVertexes[0].Point);

                FreeVector currentSide = new FreeVector();

                for (int i = 1; i < oldVertexes.Count; i++)
                {
                    currentSide = new FreeVector(oldVertexes[i - 1].Point, oldVertexes[i].Point);

                    if (VectorsProcessing.IsVectorsIntersect(currentSide, bottomDivider))
                    {
                        Point intersP = VectorsProcessing.GetIntersectionPoint(currentSide, bottomDivider);
                        _newVertexes.Add(intersP);

                        if (oldVertexes[i].Point.Y > bottomDivider.StartPoint.Y)//если вектор входит в нужную часть полуплоскости
                            _newVertexes.Add(oldVertexes[i].Point);
                    }
                    else if (oldVertexes[i].Point.Y >= bottomDivider.StartPoint.Y)//если вектор currentSide (текущая грань) Целиком лежит в нужной части полуплоскости
                    {
                        _newVertexes.Add(oldVertexes[i].Point);
                    }
                }

                //рассматриваем последнюю грань - вектор от последней точки многоугольника к первой.
                //Т.к. первая точка уже проверялась, то проверяем только пересечение
                currentSide = new FreeVector(oldVertexes.Last().Point, oldVertexes.First().Point);

                if (VectorsProcessing.IsVectorsIntersect(currentSide, bottomDivider))
                {
                    Point intersP = VectorsProcessing.GetIntersectionPoint(currentSide, bottomDivider);
                    _newVertexes.Add(intersP);
                }

                toReturn.FigureVertexes = (from p in _newVertexes select new PieceVertex(p)).ToList();
            }

            return toReturn;
        }

        /// <summary>
        /// Метод для обрезания осколка (многоугольника) правой полуплоскостью квадрата.
        /// </summary>
        /// <param name="old">Исходный осколок</param>
        /// <param name="leftDivider">Вектор - правая граница левой полуплоскости</param>
        public static Piece TrimLeft(Piece old, FreeVector leftDivider)
        {
            Piece toReturn = new Piece(old.PieceColor);

            var oldVertexes = old.FigureVertexes;

            if (oldVertexes.Count > 0)
            {
                List<Point> _newVertexes = new List<Point>();

                if (oldVertexes[0].Point.X >= leftDivider.StartPoint.X)
                    _newVertexes.Add(oldVertexes[0].Point);

                FreeVector currentSide = new FreeVector();

                for (int i = 1; i < oldVertexes.Count; i++)
                {
                    currentSide = new FreeVector(oldVertexes[i - 1].Point, oldVertexes[i].Point);

                    if (VectorsProcessing.IsVectorsIntersect(currentSide, leftDivider))
                    {
                        Point intersP = VectorsProcessing.GetIntersectionPoint(currentSide, leftDivider);
                        _newVertexes.Add(intersP);

                        if (oldVertexes[i].Point.X > leftDivider.StartPoint.X)//если вектор входит в нужную часть полуплоскости
                            _newVertexes.Add(oldVertexes[i].Point);
                    }
                    else if (oldVertexes[i].Point.X >= leftDivider.StartPoint.X)//если вектор currentSide (текущая грань) Целиком лежит в нужной части полуплоскости
                    {
                        _newVertexes.Add(oldVertexes[i].Point);
                    }
                }

                //рассматриваем последнюю грань - вектор от последней точки многоугольника к первой.
                //Т.к. первая точка уже проверялась, то проверяем только пересечение
                currentSide = new FreeVector(oldVertexes.Last().Point, oldVertexes.First().Point);

                if (VectorsProcessing.IsVectorsIntersect(currentSide, leftDivider))
                {
                    Point intersP = VectorsProcessing.GetIntersectionPoint(currentSide, leftDivider);
                    _newVertexes.Add(intersP);
                }

                toReturn.FigureVertexes = (from p in _newVertexes select new PieceVertex(p)).ToList();
            }

            return toReturn;
        }
    }

    public enum TreeDirection
    {
        Up,
        Right,
        Down,
        Left
    }

    public class FigureNotClosedException : Exception
    {
        public FigureNotClosedException() : base() { }

        public FigureNotClosedException(string message) : base(message) { }

        public FigureNotClosedException(string message, Exception innerException) : base(message, innerException) { }

        public FigureNotClosedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

}
