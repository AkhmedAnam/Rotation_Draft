using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using RotationDraft;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace ImageRotationTest
{
    [TestClass]
    public class VectorProcessingTest
    {
        [TestMethod]
        public void IsVectorsIntersectTest()
        {

            #region Без пересечения

            FreeVector v1NI = new FreeVector(new Point(-5, -2), new Point(-3, -1)),
                v2NI = new FreeVector(new Point(-3, 3), new Point(-1, -1)),
                v3NI = new FreeVector(new Point(-1, -3), new Point(5, 1)),
                v4NI = new FreeVector(new Point(2, 5), new Point(3, 2));

            bool v1v2NIResult = VectorsProcessing.IsVectorsIntersect(v1NI, v2NI),
                v3v4NIResult = VectorsProcessing.IsVectorsIntersect(v3NI, v4NI);//Здесь почему-то тру

            Assert.AreEqual(false, v1v2NIResult);
            Assert.AreEqual(false, v3v4NIResult);

            #endregion

            #region C пересечением

            FreeVector v1I = new FreeVector(new Point(4, 4), new Point(8, 7)),
                v2I = new FreeVector(new Point(7, 3), new Point(4, 5)),
                v3I = new FreeVector(new Point(-10, 2), new Point(-6, 5)),
                v4I = new FreeVector(new Point(-10, 3), new Point(-7, 3));

            bool v1v2IResult = VectorsProcessing.IsVectorsIntersect(v1I, v2I),
                v3v4IResult = VectorsProcessing.IsVectorsIntersect(v3I, v4I);

            Assert.AreEqual(true, v1v2IResult);
            Assert.AreEqual(true, v3v4IResult);

            #endregion
        }

        [TestMethod]
        public void AreVectorsFormClosedFigureTest()
        {
            #region Вектора не образующие замкнутую фигуру

            FreeVector[] vectorsAlongXAxis = new FreeVector[]
            {
                new FreeVector(new Point(2, 2), new Point(4, 2)),
                new FreeVector(new Point(4,2), new Point(6, 2)),
                new FreeVector(new Point(6, 2), new Point(8, 2))
            },

            vectorsAlongYAxis = new FreeVector[]
            {
                new FreeVector(new Point(2,2), new Point(2, 4)),
                new FreeVector(new Point(2,4), new Point(2, 6)),
                new FreeVector(new Point(2,6), new Point(2, 8)),
            },

            vectorsAlongRandomLine = new FreeVector[]
            {
                new FreeVector(new Point(2, 2), new Point(3, 4)),
                new FreeVector(new Point(3, 4), new Point(4, 6)),
                new FreeVector(new Point(4, 6), new Point(5, 8)),
            };

            #endregion

            #region Вектора образующие замкнутую фигуру

            FreeVector[] vectorFromColsedFig = new FreeVector[]
            {
                new FreeVector(new Point(2, 2), new Point(4, 2)),
                new FreeVector(new Point(4,2), new Point(6, 2)),
                new FreeVector(new Point(6, 2), new Point(7, 4))
            },

            vectorFromColsedFig1 = new FreeVector[]
            {
                new FreeVector(new Point(-6, 1), new Point(-4, 2)),
                new FreeVector(new Point(-4, 2), new Point(-2, 1)),
                new FreeVector(new Point(-2, 1), new Point(-1, 2)),
                new FreeVector(new Point(-1, 2), new Point(2, 1)),
            };

            #endregion

            bool resultAlongXAxis = VectorsProcessing.AreVectorsFormClosedFigure(vectorsAlongXAxis),
                resultAlongYAxis = VectorsProcessing.AreVectorsFormClosedFigure(vectorsAlongYAxis),
                resultAlongRandAxis = VectorsProcessing.AreVectorsFormClosedFigure(vectorsAlongRandomLine),
                truResult = VectorsProcessing.AreVectorsFormClosedFigure(vectorFromColsedFig),
                truResult1 = VectorsProcessing.AreVectorsFormClosedFigure(vectorFromColsedFig1);

            Assert.AreEqual(false, resultAlongXAxis);
            Assert.AreEqual(false, resultAlongYAxis);
            Assert.AreEqual(false, resultAlongRandAxis);

            Assert.AreEqual(true, truResult);
            Assert.AreEqual(true, truResult1);

        }

        [TestMethod]
        public void GetIntersectionPointTest()
        {
            FreeVector v1 = new FreeVector(new Point(3, 6), new Point(6, 3)),
                v2 = new FreeVector(new Point(3, 2), new Point(6, 5)),
                v3 = new FreeVector(new Point(-3, -2), new Point(6, 1)),
                v4 = new FreeVector(new Point(-1, 0), new Point(1, -2));

            Point intersectionPoint1 = VectorsProcessing.GetIntersectionPoint(v2, v1),
                interserctionPoint2 = VectorsProcessing.GetIntersectionPoint(v3, v4),
                trueResult1 = new Point(5, 4),
                trueResult2 = new Point(0, -1);

            Assert.AreEqual(trueResult1, intersectionPoint1);
            Assert.AreEqual(trueResult2, interserctionPoint2);
        }

        [TestMethod]
        public void CalculateAreaOfClosedFigureTest()
        {
            FreeVector[] figure =
            {
                new FreeVector(new Point(1, 1), new Point(3, 3)),
                new FreeVector(new Point(3, 3), new Point(4, 2)),
                new FreeVector(new Point(4, 2), new Point(5, 0)),
                new FreeVector(new Point(5, 0), new Point(1, 1))
            };

            double trueArea = 5.5d, actualArea = VectorsProcessing.CalculateAreaOfClosedFigure(figure);

            Assert.AreEqual(trueArea, actualArea);
        }
    }

    [TestClass]
    public class ImagePresenterInLayerTest
    {
        private static WriteableBitmap m_testImg;
        private static ImagePresenterInLayer m_imgPresenter;

        static ImagePresenterInLayerTest()
        {
            m_testImg = new WriteableBitmap(5, 5, 96, 96, PixelFormats.Bgr24, null);//создаю битмап размером 5*5
            byte[] pixData = new byte[]
                {
                    255, 0, 0, 0, 255, 0, 0, 0, 255, 0, 0, 0, 0, 0, 0,//синий пиксель - зелённый пиксель - красный пиксель - два чёрный пикселя

                    255, 0, 0, 0, 255, 0, 0, 0, 255, 0, 0, 0, 0, 0, 0,//синий пиксель - зелённый пиксель - красный пиксель - два чёрный пикселя

                    255, 0, 0, 0, 255, 0, 0, 0, 255, 0, 0, 0, 0, 0, 0,//синий пиксель - зелённый пиксель - красный пиксель - два чёрный пикселя

                    255, 0, 0, 0, 255, 0, 0, 0, 255, 0, 0, 0, 0, 0, 0,//синий пиксель - зелённый пиксель - красный пиксель - два чёрный пикселя

                    255, 0, 0, 0, 255, 0, 0, 0, 255, 0, 0, 0, 0, 0, 0,//синий пиксель - зелённый пиксель - красный пиксель - два чёрный пикселя
                };

            m_testImg.WritePixels(new Int32Rect(0, 0, 5, 5), pixData, 15, 0);

            m_imgPresenter = new ImagePresenterInLayer(m_testImg, 0);
        }

        [TestMethod]
        public void ImageInitializationTest()
        {
            LayerPixelCenterPoint[] result = new LayerPixelCenterPoint[]
            {
                new LayerPixelCenterPoint(new Point(0.5, -0.5), 0, 0, 0, 255),
                new LayerPixelCenterPoint(new Point(1.5, -0.5), 0, 0, 255, 0),
                new LayerPixelCenterPoint(new Point(2.5, -0.5), 0, 255, 0, 0),
                new LayerPixelCenterPoint(new Point(3.5, -0.5), 0, 0, 0, 0),
                new LayerPixelCenterPoint(new Point(4.5, -0.5), 0, 0, 0, 0),

                new LayerPixelCenterPoint(new Point(0.5, -1.5), 0, 0, 0, 255),
                new LayerPixelCenterPoint(new Point(1.5, -1.5), 0, 0, 255, 0),
                new LayerPixelCenterPoint(new Point(2.5, -1.5), 0, 255, 0, 0),
                new LayerPixelCenterPoint(new Point(3.5, -1.5), 0, 0, 0, 0),
                new LayerPixelCenterPoint(new Point(4.5, -1.5), 0, 0, 0, 0),

                new LayerPixelCenterPoint(new Point(0.5, -2.5), 0, 0, 0, 255),
                new LayerPixelCenterPoint(new Point(1.5, -2.5), 0, 0, 255, 0),
                new LayerPixelCenterPoint(new Point(2.5, -2.5), 0, 255, 0, 0),
                new LayerPixelCenterPoint(new Point(3.5, -2.5), 0, 0, 0, 0),
                new LayerPixelCenterPoint(new Point(4.5, -2.5), 0, 0, 0, 0),

                new LayerPixelCenterPoint(new Point(0.5, -3.5), 0, 0, 0, 255),
                new LayerPixelCenterPoint(new Point(1.5, -3.5), 0, 0, 255, 0),
                new LayerPixelCenterPoint(new Point(2.5, -3.5), 0, 255, 0, 0),
                new LayerPixelCenterPoint(new Point(3.5, -3.5), 0, 0, 0, 0),
                new LayerPixelCenterPoint(new Point(4.5, -3.5), 0, 0, 0, 0),

                new LayerPixelCenterPoint(new Point(0.5, -4.5), 0, 0, 0, 255),
                new LayerPixelCenterPoint(new Point(1.5, -4.5), 0, 0, 255, 0),
                new LayerPixelCenterPoint(new Point(2.5, -4.5), 0, 255, 0, 0),
                new LayerPixelCenterPoint(new Point(3.5, -4.5), 0, 0, 0, 0),
                new LayerPixelCenterPoint(new Point(4.5, -4.5), 0, 0, 0, 0),
            };

            bool isCentersEqual = true;

            for (int i = 0; i < result.Length; i++)
            {
                isCentersEqual = isCentersEqual && (m_imgPresenter.PixelCenters[i] == result[i]);

                if (!isCentersEqual)
                    break;
            }

            Assert.AreEqual(true, isCentersEqual);
        }

        [TestMethod]
        public void TransferPixelCentersTest()
        {
            LayerPixelCenterPoint[] result = new LayerPixelCenterPoint[]
            {
                new LayerPixelCenterPoint(new Point(2.5, -2.5), 0, 0, 0, 255),
                new LayerPixelCenterPoint(new Point(3.5, -2.5), 0, 0, 255, 0),
                new LayerPixelCenterPoint(new Point(4.5, -2.5), 0, 255, 0, 0),
                new LayerPixelCenterPoint(new Point(5.5, -2.5), 0, 0, 0, 0),
                new LayerPixelCenterPoint(new Point(6.5, -2.5), 0, 0, 0, 0),

                new LayerPixelCenterPoint(new Point(2.5, -3.5), 0, 0, 0, 255),
                new LayerPixelCenterPoint(new Point(3.5, -3.5), 0, 0, 255, 0),
                new LayerPixelCenterPoint(new Point(4.5, -3.5), 0, 255, 0, 0),
                new LayerPixelCenterPoint(new Point(5.5, -3.5), 0, 0, 0, 0),
                new LayerPixelCenterPoint(new Point(6.5, -3.5), 0, 0, 0, 0),

                new LayerPixelCenterPoint(new Point(2.5, -4.5), 0, 0, 0, 255),
                new LayerPixelCenterPoint(new Point(3.5, -4.5), 0, 0, 255, 0),
                new LayerPixelCenterPoint(new Point(4.5, -4.5), 0, 255, 0, 0),
                new LayerPixelCenterPoint(new Point(5.5, -4.5), 0, 0, 0, 0),
                new LayerPixelCenterPoint(new Point(6.5, -4.5), 0, 0, 0, 0),

                new LayerPixelCenterPoint(new Point(2.5, -5.5), 0, 0, 0, 255),
                new LayerPixelCenterPoint(new Point(3.5, -5.5), 0, 0, 255, 0),
                new LayerPixelCenterPoint(new Point(4.5, -5.5), 0, 255, 0, 0),
                new LayerPixelCenterPoint(new Point(5.5, -5.5), 0, 0, 0, 0),
                new LayerPixelCenterPoint(new Point(6.5, -5.5), 0, 0, 0, 0),

                new LayerPixelCenterPoint(new Point(2.5, -6.5), 0, 0, 0, 255),
                new LayerPixelCenterPoint(new Point(3.5, -6.5), 0, 0, 255, 0),
                new LayerPixelCenterPoint(new Point(4.5, -6.5), 0, 255, 0, 0),
                new LayerPixelCenterPoint(new Point(5.5, -6.5), 0, 0, 0, 0),
                new LayerPixelCenterPoint(new Point(6.5, -6.5), 0, 0, 0, 0),
            };

            m_imgPresenter.TransferPixelCenters(new Point(2, -2));

            bool isCentersEqual = true;

            for (int i = 0; i < result.Length; i++)
            {
                isCentersEqual = isCentersEqual && (m_imgPresenter.PixelCenters[i] == result[i]);

                if (!isCentersEqual)
                    break;
            }

            Assert.AreEqual(true, isCentersEqual);
        }

        [TestMethod]
        public void TransferCoordinateCenterSystemTest()
        {
            m_imgPresenter.OriginOfCoordinateSystem = OriginOfImageCoordinateSystem.Center;

            LayerPixelCenterPoint[] result = new LayerPixelCenterPoint[]
            {
                new LayerPixelCenterPoint(new Point(-2, 2), 0, 0, 0, 255),
                new LayerPixelCenterPoint(new Point(-1, 2), 0, 0, 255, 0),
                new LayerPixelCenterPoint(new Point(0, 2), 0, 255, 0, 0),
                new LayerPixelCenterPoint(new Point(1, 2), 0, 0, 0, 0),
                new LayerPixelCenterPoint(new Point(2, 2), 0, 0, 0, 0),

                new LayerPixelCenterPoint(new Point(-2, 1), 0, 0, 0, 255),
                new LayerPixelCenterPoint(new Point(-1, 1), 0, 0, 255, 0),
                new LayerPixelCenterPoint(new Point(0, 1), 0, 255, 0, 0),
                new LayerPixelCenterPoint(new Point(1, 1), 0, 0, 0, 0),
                new LayerPixelCenterPoint(new Point(2, 1), 0, 0, 0, 0),

                new LayerPixelCenterPoint(new Point(-2, 0), 0, 0, 0, 255),
                new LayerPixelCenterPoint(new Point(-1, 0), 0, 0, 255, 0),
                new LayerPixelCenterPoint(new Point(0, 0), 0, 255, 0, 0),
                new LayerPixelCenterPoint(new Point(1, 0), 0, 0, 0, 0),
                new LayerPixelCenterPoint(new Point(2, 0), 0, 0, 0, 0),

                new LayerPixelCenterPoint(new Point(-2, -1), 0, 0, 0, 255),
                new LayerPixelCenterPoint(new Point(-1, -1), 0, 0, 255, 0),
                new LayerPixelCenterPoint(new Point(0, -1), 0, 255, 0, 0),
                new LayerPixelCenterPoint(new Point(1, -1), 0, 0, 0, 0),
                new LayerPixelCenterPoint(new Point(2, -1), 0, 0, 0, 0),

                new LayerPixelCenterPoint(new Point(-2, -2), 0, 0, 0, 255),
                new LayerPixelCenterPoint(new Point(-1, -2), 0, 0, 255, 0),
                new LayerPixelCenterPoint(new Point(0, -2), 0, 255, 0, 0),
                new LayerPixelCenterPoint(new Point(1, -2), 0, 0, 0, 0),
                new LayerPixelCenterPoint(new Point(2, -2), 0, 0, 0, 0),
            };

            bool isCentersEqual = true;

            for (int i = 0; i < result.Length; i++)
            {
                isCentersEqual = isCentersEqual && (m_imgPresenter.PixelCenters[i] == result[i]);

                if (!isCentersEqual)
                    break;
            }

            Assert.AreEqual(true, isCentersEqual);
        }

        [TestMethod]
        public void RotateImageTest()
        {
            m_imgPresenter.OriginOfCoordinateSystem = OriginOfImageCoordinateSystem.Center;

            m_imgPresenter.RotateImage(30);

            LayerPixelCenterPoint[] result = new LayerPixelCenterPoint[]//координаты повёрнутых точек расчитаны на сайте http://www.abakbot.ru/online-2/91-rotate
            {
                new LayerPixelCenterPoint(new Point(-2.7320508075689, 0.73205080756888), 0, 0, 0, 255),
                new LayerPixelCenterPoint(new Point(-1.8660254037844, 1.2320508075689), 0, 0, 255, 0),
                new LayerPixelCenterPoint(new Point(-1, 1.7320508075689 ), 0, 255, 0, 0),
                new LayerPixelCenterPoint(new Point(-0.13397459621556, 2.2320508075689), 0, 0, 0, 0),
                new LayerPixelCenterPoint(new Point(0.73205080756888, 2.7320508075689), 0, 0, 0, 0),

                new LayerPixelCenterPoint(new Point(-2.2320508075689, -0.13397459621556 ), 0, 0, 0, 255),
                new LayerPixelCenterPoint(new Point(-1.3660254037844, 0.36602540378444 ), 0, 0, 255, 0),
                new LayerPixelCenterPoint(new Point(-0.5, 0.86602540378444), 0, 255, 0, 0),
                new LayerPixelCenterPoint(new Point(0.36602540378444, 1.3660254037844), 0, 0, 0, 0),
                new LayerPixelCenterPoint(new Point(1.2320508075689, 1.8660254037844), 0, 0, 0, 0),

                new LayerPixelCenterPoint(new Point(-1.7320508075689, -1), 0, 0, 0, 255),
                new LayerPixelCenterPoint(new Point(-0.86602540378444, -0.5), 0, 0, 255, 0),
                new LayerPixelCenterPoint(new Point(0, 0), 0, 255, 0, 0),
                new LayerPixelCenterPoint(new Point(0.86602540378444, 0.5 ), 0, 0, 0, 0),
                new LayerPixelCenterPoint(new Point(1.7320508075689, 1), 0, 0, 0, 0),

                new LayerPixelCenterPoint(new Point(-1.2320508075689, -1.8660254037844), 0, 0, 0, 255),
                new LayerPixelCenterPoint(new Point(-0.36602540378444, -1.3660254037844), 0, 0, 255, 0),
                new LayerPixelCenterPoint(new Point(0.5, -0.86602540378444), 0, 255, 0, 0),
                new LayerPixelCenterPoint(new Point(1.3660254037844, -0.36602540378444), 0, 0, 0, 0),
                new LayerPixelCenterPoint(new Point(2.2320508075689, 0.13397459621556), 0, 0, 0, 0),

                new LayerPixelCenterPoint(new Point(-0.73205080756888, -2.7320508075689), 0, 0, 0, 255),
                new LayerPixelCenterPoint(new Point(0.13397459621556, -2.2320508075689), 0, 0, 255, 0),
                new LayerPixelCenterPoint(new Point(1, -1.7320508075689), 0, 255, 0, 0),
                new LayerPixelCenterPoint(new Point(1.8660254037844, -1.2320508075689), 0, 0, 0, 0),
                new LayerPixelCenterPoint(new Point(2.7320508075689, -0.73205080756888), 0, 0, 0, 0),
            };

            foreach (var c in result)
                c.RotationAngle = 30;

            bool isResultTrue = AreArraysEqual(result, m_imgPresenter.PixelCenters.ToArray());

            Assert.AreEqual(true, isResultTrue);
        }

        [TestMethod]
        public void GetPixelsWithConditionTest()
        {
            m_imgPresenter.OriginOfCoordinateSystem = OriginOfImageCoordinateSystem.Center;

            LayerPixelCenterPoint[] gotArr = m_imgPresenter.GetPixelsWithCondition((p) => p.GetDistanceFromPoint(new Point(-1.5, 0.5)) <= 1),

                                    trueResult = new LayerPixelCenterPoint[]
                                    {
                                        new LayerPixelCenterPoint(new Point(-2, 1), 0, 0, 0, 255),
                                        new LayerPixelCenterPoint(new Point(-1, 1), 0, 0, 255, 0),
                                        new LayerPixelCenterPoint(new Point(-2, 0), 0, 0, 0, 255),
                                        new LayerPixelCenterPoint(new Point(-1, 0), 0, 0, 255, 0),
                                    };

            bool isArrEqual = AreArraysEqual(gotArr, trueResult);

            Assert.AreEqual(true, isArrEqual);
        }

        

        private bool AreArraysEqual<T>(T[] firstArr, T[] secondArr) where T : IEquatable<T>
        {
            bool toReturn = true;

            if (firstArr.Length != secondArr.Length)
                throw new ArgumentException("First array and second array must have the same length");

            for (int i = 0; i < firstArr.Length; i++)
            {

                toReturn = toReturn && firstArr[i].Equals(secondArr[i]);

                if (!toReturn)
                    break;
            }

            return toReturn;
        }
    }

    [TestClass]
    public class LayerPixelCenterPointTest
    {
        [TestMethod]
        public void PixelRotationTest()
        {
            LayerPixelCenterPoint pixelCenter = new LayerPixelCenterPoint(new Point(1, 1), 0, 255, 0, 0);

            pixelCenter.RotationAngle = 30;

            double xBL = Math.Round(pixelCenter.BottomLeft.X, 10), yBL = Math.Round(pixelCenter.BottomLeft.Y, 10),
                   xTL = Math.Round(pixelCenter.TopLeft.X, 10), yTL = Math.Round(pixelCenter.TopLeft.Y, 10),
                   xTR = Math.Round(pixelCenter.TopRight.X, 10), yTR = Math.Round(pixelCenter.TopRight.Y, 10),
                   xBR = Math.Round(pixelCenter.BottomRight.X, 10), yBR = Math.Round(pixelCenter.BottomRight.Y, 10);

            bool isNewOrientationCorrect = xBL == 0.8169872981 && yBL == 0.3169872981 &&

                                           xTL == 0.3169872981 && yTL == 1.1830127019 &&

                                           xTR == 1.1830127019 && yTR == 1.6830127019 &&

                                           xBR == 1.6830127019 && yBR == 0.8169872981;

            Assert.AreEqual(true, isNewOrientationCorrect);
        }

        [TestMethod]
        public void IsPointBelongThisPixelTest()
        {
            LayerPixelCenterPoint lpcp = new LayerPixelCenterPoint(new Point(3, 2), 0, 0, 0, 0);
            lpcp.RotationAngle = 30;

            Point pointWithinPix = new Point(2.7, 1.7), pointBeyondPix = new Point(2, 0.5);

            bool isFirstPointCorrect = lpcp.IsPointBelongThisPixel(pointWithinPix), isSecondPointCorrect = lpcp.IsPointBelongThisPixel(pointBeyondPix);

            Assert.AreEqual(true, isFirstPointCorrect);
            Assert.AreEqual(false, isSecondPointCorrect);

        }
    }


    [TestClass]
    public class PieceTest
    {
        [TestMethod]
        public void AreaTest()
        {
            Piece p = new Piece(Color.FromRgb(0, 0, 0));
            PieceVertex[] vs =
            {
                new PieceVertex(new Point(1, 1), RelatedPixelSide.BetweenFirstAndSecondSide),
                new PieceVertex(new Point(3, 3), RelatedPixelSide.BetweenFirstAndSecondSide),
                new PieceVertex(new Point(4, 2), RelatedPixelSide.BetweenFirstAndSecondSide),
                new PieceVertex(new Point(5, 0), RelatedPixelSide.BetweenFirstAndSecondSide),
            };

            p.FigureVertexes.AddRange(vs);

            double trueArea = 5.5d, actualArea = p.Area;

            Assert.AreEqual(trueArea, actualArea);
        }

        [TestMethod]
        public void TrimTest()
        {
            Piece toTrim = new Piece(Colors.Violet);

            Point[] figure = new Point[]
            {
                new Point(4,0),
                new Point(3,2),
                new Point(5,3),
                new Point(6,1)
            };


            toTrim.FigureVertexes.AddRange((from p in figure select new PieceVertex(p)).ToArray());

            FreeVector topDivider = new FreeVector(new Point(-7, 2), new Point(15, 2)),
                rightDivider = new FreeVector(new Point(5, 12), new Point(5, -10)),
                bottomDivider = new FreeVector(new Point(15, 0), new Point(-7, 0)),
                leftDivider = new FreeVector(new Point(3, -10), new Point(3, 12));

            toTrim = Piece.TrimTop(toTrim, topDivider);
            toTrim = Piece.TrimRight(toTrim, rightDivider);
            toTrim = Piece.TrimBottom(toTrim, bottomDivider);
            toTrim = Piece.TrimLeft(toTrim, leftDivider);

            Piece truePiece = new Piece(Colors.Violet);

            figure = new Point[]
            {
                new Point(3,2),
                new Point(5,2),
                new Point(5,0.5),
                new Point(4,0)
            };

            truePiece.FigureVertexes.AddRange((from p in figure select new PieceVertex(p)).ToArray());

            bool m_isOK_forFirst = truePiece == toTrim;

            figure = new Point[]
                {
                    new Point(4,1),
                    new Point(2,3),
                    new Point(4,5),
                    new Point(6,3)
                };

            toTrim.FigureVertexes = new System.Collections.Generic.List<PieceVertex>();
            toTrim.FigureVertexes.AddRange((from p in figure select new PieceVertex(p)).ToArray());

            figure = new Point[]
            {
                new Point(3,2),
                new Point(4,2),
                new Point(4,1)
            };

            truePiece.FigureVertexes = new System.Collections.Generic.List<PieceVertex>();
            truePiece.FigureVertexes.AddRange((from p in figure select new PieceVertex(p)).ToArray());


            topDivider = new FreeVector(new Point(-8, 2), new Point(14, 2));
            rightDivider = new FreeVector(new Point(4, 12), new Point(4, -10));
            bottomDivider = new FreeVector(new Point(14, 0), new Point(-8, 0));
            leftDivider = new FreeVector(new Point(2, -10), new Point(2, 12));

            toTrim = Piece.TrimTop(toTrim, topDivider);
            toTrim = Piece.TrimRight(toTrim, rightDivider);
            toTrim = Piece.TrimBottom(toTrim, bottomDivider);
            toTrim = Piece.TrimLeft(toTrim, leftDivider);

            bool m_isOk_forSecond = truePiece == toTrim;

            Assert.AreEqual(true, m_isOK_forFirst);
            Assert.AreEqual(true, m_isOk_forSecond);
        }
    }

    [TestClass]
    public class ImagesLayerTest
    {
        [TestMethod]
        public void GetNewPixelCenterPoint_Test()
        {
            LayerPixelCenterPoint old = new LayerPixelCenterPoint(new Point(1.5, 2), 0, 80, 80, 80),
                first = new LayerPixelCenterPoint(new Point(2, 2.5), 1, 40, 40, 40),
                second = new LayerPixelCenterPoint(new Point(2, 1.5), 1, 120, 120, 120),
                third = new LayerPixelCenterPoint(new Point(1, 2.5), 1, 160, 160, 160),
                fourth = new LayerPixelCenterPoint(new Point(1, 1.5), 1, 240, 240, 240),
                result = new LayerPixelCenterPoint(new Point(1.5, 2), 0, 140, 140, 140);

            ImagesLayer l = new ImagesLayer(5, 5);

            var m_new = l.GetNewPixelCenterPoint(old, first, second, third, fourth);

            bool m_isOK = m_new == result;

            Assert.AreEqual(true, m_isOK);
        }

    }

}
