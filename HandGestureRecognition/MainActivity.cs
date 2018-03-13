using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Util;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using Emgu.CV.Cuda;
using System.IO.MemoryMappedFiles;
using Emgu.CV.ML;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using Emgu.CV.ML.MlEnum;
using System.Speech.Synthesis;

namespace HandGestureRecognition
{
    public partial class MainActivity : Form
    {
        #region declaration
        VideoWriter VideoW;
        int adasas = 0;
        char[] array = new char[] { 'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z' };
        string frameName;
        int frameNumber = 1;
        int first = -1;
        int last = -1;
        VideoCapture capture;
        Boolean Pause = false;
        Boolean isFirst = false;
        Boolean isLast = true;
        Matrix<float> response = new Matrix<float>(16, 26) { };

        #endregion

        Boolean maximized = false;
        int posX;
        int posY;
        bool drag;

        public MainActivity()
        {
            InitializeComponent();
        }

        private void exit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void minimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                drag = true;
                posX = Cursor.Position.X - this.Left;
                posY = Cursor.Position.Y - this.Top;
            }
        }
        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            drag = false;
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (drag)
            {
                this.Top = System.Windows.Forms.Cursor.Position.Y - posY;
                this.Left = System.Windows.Forms.Cursor.Position.X - posX;
            }
            this.Cursor = Cursors.Default;
        }

        private void openVideoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            System.IO.File.WriteAllText(@"g.txt", " ");
            ofd.Filter = "MP4 Files (.mp4)|*.mp4";
            adasas = 0;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                frameNumber = 0;
                capture = null;
                capture = new VideoCapture(ofd.FileName);
                Mat m = new Mat();
                capture.Read(m);
                pictureBox1.Image = m.Bitmap;
            }
        }

        private async void start_Click(object sender, EventArgs e)
        {
            if (capture == null)
            {
                return;
            }
            //try
            //{
                double frameNumber = capture.GetCaptureProperty(CapProp.FrameCount);
                float[] smoothgrad = new float[(int)frameNumber];
                //label1.Text = "Frame Count is : " + frameNumber.ToString();
                VideoW = new VideoWriter(@"temp.avi",
                                    FourCC.H264/*VideoWriter.Fourcc('M','J','P','G')Convert.ToInt32(capture.GetCaptureProperty(CapProp.FourCC))*/,
                                    30,
                                    new Size(capture.Width, capture.Height),
                                    true);
                while (!Pause)
                {
                    Mat matInput = new Mat();
                    capture.Read(matInput);
                    if (!matInput.IsEmpty)
                    {

                        moduleKeyFrameExtraction(matInput);
                        double fps = capture.GetCaptureProperty(CapProp.Fps);
                        await Task.Delay(1000 / Convert.ToInt32(fps));
                    }
                    else
                    {
                        break;
                    }
                }
            /*}
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }*/
        }

        public bool skinAreaDetection(Bitmap b)
        {
            for (int i = 0; i < b.Width; i++)
            {
                for (int j = 0; j < b.Height; j++)
                {
                    Color c1 = b.GetPixel(i, j);
                    int r1 = c1.R;
                    int g1 = c1.G;
                    int b1 = c1.B;
                    int a1 = c1.A;
                    int gray = (byte)(.299 * r1 + .587 * g1 + .114 * b1);
                    float hue = c1.GetHue();
                    float sat = c1.GetSaturation();
                    float val = c1.GetBrightness();
                    if (0.0 <= hue && hue <= 50.0 && 0.1 <= sat && sat <= 1 /*&& val > 130*/ && r1 > 95 && g1 > 40 && b1 > 20 && r1 > g1 && r1 > b1 && Math.Abs(r1 - g1) > 15 && a1 > 15)
                    {
                        b.SetPixel(i, j, Color.FromArgb(gray, gray, gray));
                    }
                    else
                    {
                        b.SetPixel(i, j, Color.FromArgb(0, 0, 0));
                    }
                }
            }
            return true;
        }

        private void moduleKeyFrameExtraction(Mat inputMat)
        {

            Image<Bgr, Byte> imageMedianBlur = modulePreProcessing(inputMat);
            //MedianBlur for Feature Extraction
            /*Image<Bgr, Byte> imageFE = new Image<Bgr, Byte>(inputMat.Width, inputMat.Height);
            CvInvoke.MedianBlur(imageInput, imageFE, 7);
            pictureBox2.Image = imageFE.Bitmap;*/

            //Sobel
            Image<Gray, float> imageSobelInput = imageMedianBlur.Convert<Gray, float>();
            Image<Gray, float> _imgSobelx = new Image<Gray, float>(imageSobelInput.Width, imageSobelInput.Height);
            Image<Gray, float> _imgSobely = new Image<Gray, float>(imageSobelInput.Width, imageSobelInput.Height);
            _imgSobelx = imageSobelInput.Sobel(1, 0, 3);
            _imgSobely = imageSobelInput.Sobel(0, 1, 3);
            Image<Gray, float> magnitude = new Image<Gray, float>(imageSobelInput.Width, imageSobelInput.Height);
            Image<Gray, float> angle = new Image<Gray, float>(imageSobelInput.Width, imageSobelInput.Height);
            CvInvoke.CartToPolar(_imgSobelx, _imgSobely, magnitude, angle, true);
            //Image box2 image of gradient
            //imageBox3.Image = magnitude;
            //FrameNumber Display
            //label2.Text = frameNumber.ToString();
            double avg = magnitude.GetAverage().Intensity;
            //Chart of gradient
            //chart1.Series["Gradient"].Points.AddXY(frameNumber, avg);
            if (!isFirst)
            {
                if (avg > 0)
                {
                    isFirst = true;
                    first = frameNumber;
                    isLast = false;
                }
            }
            if (!isLast)
            {
                if (avg == 0)
                {
                    isLast = true;
                    last = frameNumber;
                    //label3.Text = "First: " + first + " Last: " + last;
                    int mid = (first + last) / 2;
                    moduleFeatureExtraction(first, last);
                    isFirst = false;
                }
            }
            frameNumber++;
        }

        private Image<Bgr, byte> modulePreProcessing(Mat inputMat)
        {

            //Actual video is played in pictureBox1
            Image<Bgr, Byte> imageInput = inputMat.ToImage<Bgr, Byte>();
            pictureBox1.Image = imageInput.Bitmap;


            //Face elimination
            imageInput = inputMat.ToImage<Bgr, Byte>();
            Image<Gray, byte> grayframe = imageInput.Convert<Gray, byte>();
            CascadeClassifier face = new CascadeClassifier("C:\\Emgu\\emgucv-windesktop 3.3.0.2824\\opencv\\data\\haarcascades\\haarcascade_frontalface_default.xml");
            var faces = face.DetectMultiScale(grayframe, 1.1, 25, new Size(10, 10));
            foreach (var f in faces)
            {
                Rectangle faceRectangle = Rectangle.Inflate(f, 40, 40);
                imageInput.Draw(faceRectangle, new Bgr(Color.Black), -1);
            }
            Mat faceEliminationMat = imageInput.Mat;
            Image<Bgr, Byte> faceElimination = faceEliminationMat.ToImage<Bgr, Byte>();
            //pictureBox2.Image = faceElimination.Bitmap; //Face elimination


            //Skin area detection

            skinAreaDetection(imageInput.Bitmap);
            //imageBox1.Image = imageInput;


            Image<Bgr, Byte> imageMedianBlurForExtraction = new Image<Bgr, Byte>(inputMat.Width, inputMat.Height);
            CvInvoke.MedianBlur(imageInput, imageMedianBlurForExtraction, 7);
            //imageBox2.Image = imageMedianBlurForExtraction; //Noise Removing
            Image<Bgr, Byte> real = resize(imageMedianBlurForExtraction);
            frameName = "gesture\\" + frameNumber + ".jpeg";
            real.Save(frameName);

            //MedianBlur for KeySize(imageMedianBlur.Width, imageMedianBlur.Height) Frame Extraction
            Image<Bgr, Byte> imageMedianBlur = new Image<Bgr, Byte>(inputMat.Width, inputMat.Height);
            CvInvoke.MedianBlur(imageInput, imageMedianBlur, 21);



            return imageMedianBlur;
        }

        private async void moduleFeatureExtraction(int first, int last)
        {
            string fghfh = "";
            double[,] RawData = new double[16, 3780];
            int mid = (first + last) / 2;
            int low = mid - 8; ;
            int high = mid + 8;
            for (int i = 0; i < 16; i++)
            {
                for (int j = 0; j < 26; j++)
                {
                    if (j == adasas)
                        response[i, j] = 1;
                    if (j != adasas)
                        response[i, j] = 0;
                }
            }
            adasas++;
            if (low < first)
                low++;
            if (high > last)
                low++;
            int length = high - low;
            for (int k = (low); k < (high); k++)
            {
                string frameName = "gesture//" + k + ".jpeg";
                Image<Bgr, byte> featurExtractionInput = new Image<Bgr, byte>(frameName);
                //pictureBox3.Image = featurExtractionInput.Bitmap;
                //label4.Text = k.ToString();
                await Task.Delay(1000 / Convert.ToInt32(2));
                float[] desc = new float[3780];
                desc = GetVector(featurExtractionInput);

                int i = k - (low);
                for (int j = 0; j < 3780; j++)
                {
                    double val = Convert.ToDouble(desc[j]);
                    RawData.SetValue(val, i, j);
                }

                if (k == (high - 1))
                {
                    Matrix<Double> DataMatrix = new Matrix<Double>(RawData);
                    Matrix<Double> Mean = new Matrix<Double>(1, 3780);
                    Matrix<Double> EigenValues = new Matrix<Double>(1, 3780);
                    Matrix<Double> EigenVectors = new Matrix<Double>(3780, 3780);
                    CvInvoke.PCACompute(DataMatrix, Mean, EigenVectors, 16);
                    Matrix<Double> result = new Matrix<Double>(16, 16);
                    CvInvoke.PCAProject(DataMatrix, Mean, EigenVectors, result);


                    String filePath = @"test.xml";
                    StringBuilder sb = new StringBuilder();
                    (new XmlSerializer(typeof(Matrix<double>))).Serialize(new StringWriter(sb), result);
                    XmlDocument xDoc = new XmlDocument();
                    xDoc.LoadXml(sb.ToString());

                    System.IO.File.WriteAllText(filePath, sb.ToString());
                    Matrix<double> matrix = (Matrix<double>)(new XmlSerializer(typeof(Matrix<double>))).Deserialize(new XmlNodeReader(xDoc));

                    string djf = null;
                    djf = System.IO.File.ReadAllText(@"g.txt");
                    djf += Environment.NewLine;
                    djf += Environment.NewLine;
                    for (int p = 0; p < 16; p++)
                    {
                        for (int q = 0; q < 16; q++)
                        {
                            djf += p + " , " + q + "  " + matrix[p, q].ToString() + "    ";
                        }
                        djf += Environment.NewLine;
                    }
                    Matrix<float> masjhdb = result.Convert<float>();
                    TrainData trainData = new TrainData(masjhdb, DataLayoutType.RowSample, response);
                    int features = 16;
                    int classes = 26;
                    Matrix<int> layers = new Matrix<int>(6, 1);
                    layers[0, 0] = features;
                    layers[1, 0] = classes * 16;
                    layers[2, 0] = classes * 8;
                    layers[3, 0] = classes * 4;
                    layers[4, 0] = classes * 2;
                    layers[5, 0] = classes;
                    ANN_MLP ann = new ANN_MLP();
                    FileStorage fileStorageRead = new FileStorage(@"abc.xml", FileStorage.Mode.Read);
                    ann.Read(fileStorageRead.GetRoot(0));
                    ann.SetLayerSizes(layers);
                    ann.SetActivationFunction(ANN_MLP.AnnMlpActivationFunction.SigmoidSym, 0, 0);
                    ann.SetTrainMethod(ANN_MLP.AnnMlpTrainMethod.Backprop, 0, 0);
                    ann.Train(masjhdb, DataLayoutType.RowSample, response);
                    FileStorage fileStorageWrite = new FileStorage(@"abc.xml", FileStorage.Mode.Write);
                    ann.Write(fileStorageWrite);
                    Matrix<float> hehe = new Matrix<float>(1, 16);
                    for (int q = 0; q < 16; q++)
                    {
                        hehe[0, q] = masjhdb[11, q];
                    }
                    float real = ann.Predict(hehe);

                    fghfh += array[(int)real];
                    SpeechSynthesizer reader = new SpeechSynthesizer();


                    if (richTextBox1.Text != " ")
                    {
                        reader.Dispose();
                        reader = new SpeechSynthesizer();
                        reader.SpeakAsync(fghfh.ToString());
                    }
                    else
                    {
                        MessageBox.Show("No Text Present!");
                    }
                    richTextBox1.Text = fghfh.ToString();
                    System.IO.File.WriteAllText(@"g.txt", real.ToString());
                }
            }
        }

        public Image<Bgr, Byte> resize(Image<Bgr, Byte> im)
        {
            return im.Resize(64, 128, Emgu.CV.CvEnum.Inter.Linear);
        }

        public float[] GetVector(Image<Bgr, Byte> imageOfInterest)
        {
            HOGDescriptor hog = new HOGDescriptor();    // with defaults values
            System.Drawing.Point[] p = new System.Drawing.Point[imageOfInterest.Width * imageOfInterest.Height];
            int k = 0;
            for (int i = 0; i < imageOfInterest.Width; i++)
            {
                for (int j = 0; j < imageOfInterest.Height; j++)
                {
                    System.Drawing.Point p1 = new System.Drawing.Point(i, j);
                    p[k++] = p1;
                }
            }
            float[] result = hog.Compute(imageOfInterest, new System.Drawing.Size(16, 16), new System.Drawing.Size(0, 0), p);
            return result;
        }
        
        /*
Boolean maximized = false;

int posX;
int posY;
bool drag;

private void panel1_MouseDoubleClick(object sender, MouseEventArgs e)
{
   if (e.Button == MouseButtons.Left)
   {
       if (maximized)
       {
           this.WindowState = FormWindowState.Normal;
           maximized = false;
       }
       else
       {
           this.WindowState = FormWindowState.Maximized;
           maximized = true;
       }
   }
}

private void panel1_MouseDown(object sender, MouseEventArgs e)
{
   if (e.Button == MouseButtons.Left)
   {
       drag = true;
       posX = Cursor.Position.X - this.Left;
       posY = Cursor.Position.Y - this.Top;
   }
}

private void panel1_MouseUp(object sender, MouseEventArgs e)
{
   drag = false;
}

private void panel1_MouseMove(object sender, MouseEventArgs e)
{
   if (drag)
   {
       this.Top = System.Windows.Forms.Cursor.Position.Y - posY;
       this.Left = System.Windows.Forms.Cursor.Position.X - posX;
   }
   this.Cursor = Cursors.Default;
}

private void exit_Click(object sender, EventArgs e)
{
   this.Close();
}

private void minimize_Click(object sender, EventArgs e)
{
   this.WindowState = FormWindowState.Minimized;
}

private void maximize_Click(object sender, EventArgs e)
{
   if (maximized)
   {
       maximized = false;
       this.WindowState = FormWindowState.Normal;
   }
   else
   {
       maximized = true;
       this.WindowState = FormWindowState.Maximized;
   }
}*/
    }
}
