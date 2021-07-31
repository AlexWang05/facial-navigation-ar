using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCvSharp;
using System;


public class FaceDetector : MonoBehaviour
{
    WebCamTexture _webCamTexture;
    public CascadeClassifier faceCascade;
    public CascadeClassifier leftEyeCascade;
    public TextAsset faceXML;
    public TextAsset eyeXML;
    public GameObject faceCube;
    public GameObject playerCamera;
    private GameObject PinP;
    private Timer theTimer;
    private float cubeLocationX;
    private float cubeLocationY;
    private int videoWidth = 512;
    private int videoHeight = 288;
    private int videoFPS = 10;
    OpenCvSharp.Rect MyFace;
    OpenCvSharp.Rect Eye1;
    OpenCvSharp.Rect Eye2;
    private float eye1X;
    private float eye1Y;
    private float eye2X;
    private float eye2Y;
    public int cameraDevice = 0;

    public float speed = 3.0f;

    // Start is called before the first frame update
    void Start()
    {
        theTimer = GameObject.Find("Canvas").GetComponent<Timer>();
        PinP = GameObject.Find("RawImage");
        WebCamDevice[] devices = WebCamTexture.devices;

        Debug.Log(devices[cameraDevice].kind);

        _webCamTexture = new WebCamTexture(devices[cameraDevice].name);
        _webCamTexture.requestedWidth = videoWidth;
        _webCamTexture.requestedHeight = videoHeight;
        _webCamTexture.requestedFPS = videoFPS;
        _webCamTexture.Play();
        faceCascade = new CascadeClassifier("Assets/haarcascade_frontalface_default.xml");
        // faceCascade = new CascadeClassifier("Assets/haarcascade_profileface.xml");
        leftEyeCascade = new CascadeClassifier("Assets/haarcascade_righteye_2splits.xml");
    }

    // Update is called once per frame
    void LateUpdate()
    {
        GameObject canvas = GameObject.Find("Canvas");
        Timer playerScript = canvas.GetComponent<Timer>();

        if(!playerScript.timeOut){
            Destroy(FindObjectOfType<Texture2D>());
            Mat frame = OpenCvSharp.Unity.TextureToMat(_webCamTexture);
            // frame = frame.Flip(FlipMode.Y);
            findNewFace(frame);
            findLeftEye(frame);
            display(frame);
        }
        
        
    }

    void findLeftEye(Mat frame)
    {
        var eyes = leftEyeCascade.DetectMultiScale(frame, 1.1, 2, HaarDetectionType.ScaleImage);

        if (eyes.Length >= 2)
        {
            // Debug.Log(faces[0].Location);
            eye1X = eyes[0].Location.X;
            eye1Y = eyes[0].Location.Y;
            eye2X = eyes[1].Location.X;
            eye2Y = eyes[1].Location.Y;
            Eye1 = eyes[0];
            Eye2 = eyes[1];
        }

        // Debug.Log(Eye1.Intersect(Eye2).Size);
    }
    void findNewFace(Mat frame)
    {
        var faces = faceCascade.DetectMultiScale(frame, 1.1, 2, HaarDetectionType.ScaleImage);

        if (faces.Length >= 1)
        {
            // Debug.Log(faces[0].Location);
            cubeLocationX = faces[0].Location.X;
            cubeLocationY = faces[0].Location.Y;
            MyFace = faces[0];
        }
    }

    void display(Mat frame)
    {
        if (MyFace != null && Eye1 != null && Eye2 != null)
        {
            if(MyFace.Size.Width * MyFace.Size.Height > (Eye1.Size.Width * Eye1.Size.Height + Eye2.Size.Width * Eye2.Size.Height) &&
                Eye1.Intersect(Eye2).Size.Height * Eye1.Intersect(Eye2).Size.Width < 10 &&
                MyFace.Intersect(Eye1).Size.Height * MyFace.Intersect(Eye1).Size.Width > 0 &&
                MyFace.Intersect(Eye2).Size.Height * MyFace.Intersect(Eye2).Size.Width > 0 &&
                Math.Abs(eye1Y - eye2Y) < Eye1.Size.Height){

                float difference = 0f;

                if(eye1X < eye2X){
                    difference = (eye1Y - eye2Y);
                } else if(eye2X < eye1X){
                    difference = (eye2Y - eye1Y);
                }
                // if(Math.Abs(difference) < 7){
                //     difference = 0;
                // }

                frame.Rectangle(MyFace, new Scalar(0, 255, 0), 2);
                frame.Rectangle(Eye1, new Scalar(0, 255, 0), 2);
                frame.Rectangle(Eye2, new Scalar(0, 255, 0), 2);

                translateCamera(difference);
            }
            Texture newtexture = OpenCvSharp.Unity.MatToTexture(frame.Flip(FlipMode.Y));
            // GetComponent<Renderer>().material.mainTexture = newtexture;

            float pipScale = .4f;

            PinP.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(-newtexture.width*pipScale * .7f, newtexture.height*pipScale*.7f, 0);
            PinP.GetComponent<RectTransform>().sizeDelta = new Vector2(newtexture.width*pipScale, newtexture.height*pipScale);
            PinP.GetComponent<CanvasRenderer>().SetTexture(newtexture);
            
        }
    }

    void translateCamera(float rotate){
        // Debug.Log(MyFace.Size.Height * MyFace.Size.Width);
        // Debug.Log(cubeLocationX);

        if(MyFace.Size.Height * MyFace.Size.Width < (0.35 * _webCamTexture.height) * (0.25 * _webCamTexture.width))
        {
            playerCamera.transform.Translate(Vector3.back * Time.deltaTime * speed);
        }
        if (MyFace.Size.Height * MyFace.Size.Width > (0.45*_webCamTexture.height)*(0.3*_webCamTexture.width))
        {
            playerCamera.transform.Translate(Vector3.forward * Time.deltaTime * speed);
        }
        // if(cubeLocationX < 170){
        //     playerCamera.transform.Translate(Vector3.right * Time.deltaTime * speed);
        // }
        // if(cubeLocationX > 260){
        //     playerCamera.transform.Translate(Vector3.left * Time.deltaTime * speed);
        // }
        // Debug.Log(rotate/MyFace.Size.Height);
        if(Math.Abs(rotate/MyFace.Size.Height) > 0.04f){
            playerCamera.transform.Rotate(Vector3.up, rotate/MyFace.Size.Height * Time.deltaTime * 500f);
        }
            
    }
}

