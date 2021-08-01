using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCvSharp;
using System;


public class FaceDetector : MonoBehaviour
{
    public bool performHandDetection = false;

    // first camera device in array
    public int cameraDevice = 0;
    private GameObject cameraImage;

    WebCamTexture _webCamTexture;

    // width and height at which the video is processed at
    private int videoWidth = 512;
    private int videoHeight = 288;

    // cascades used to recognize face, eyes, hand
    public CascadeClassifier faceCascade;
    public CascadeClassifier leftEyeCascade;
    public CascadeClassifier handCascade;

    public GameObject playerCamera;
    
    // locations of face square
    private float cubeLocationX;
    private float cubeLocationY;

    // request fps
    private int videoFPS = 10;
    
    // rectangles to render
    OpenCvSharp.Rect MyFace;
    OpenCvSharp.Rect Eye1;
    OpenCvSharp.Rect Eye2;

    // locations of eyes
    private float eye1X;
    private float eye1Y;
    private float eye2X;
    private float eye2Y;

    // location of hand
    private float hand1X;
    private float hand1Y;

    OpenCvSharp.Rect Hand1;

    public GameObject screen;

    public float speed = 3.0f;

    // Start is called before the first frame update
    void Start()
    {
        cameraImage = GameObject.Find("RawImage");
        WebCamDevice[] devices = WebCamTexture.devices;

        // texture on which the video will be rendered
        _webCamTexture = new WebCamTexture(devices[cameraDevice].name);
        
        // set width, height, and fps
        _webCamTexture.requestedWidth = videoWidth;
        _webCamTexture.requestedHeight = videoHeight;
        _webCamTexture.requestedFPS = videoFPS;

        _webCamTexture.Play();
        
        // load classifier xml file provided by the OpenCV library
        faceCascade = new CascadeClassifier("Assets/haarcascade_frontalface_default.xml");
        leftEyeCascade = new CascadeClassifier("Assets/haarcascade_righteye_2splits.xml");

        // cascade ranking: fist.xml (works fine sometimes), aGest.xml (not so good), hand.xml (not so good)

        if (performHandDetection)
            handCascade = new CascadeClassifier("Assets/handcascade.xml");
    }

    // LateUpdate is used because update only needs to happen when frame is updated
    void LateUpdate()
    {
        // destroy texture in current picture
        Destroy(FindObjectOfType<Texture2D>());

        // start making a new texture
        // converts webcam texture to Mat
        Mat frame = OpenCvSharp.Unity.TextureToMat(_webCamTexture);

        // find and save locations of face and eyes
        findNewFace(frame);
        FindEyes(frame);

        if (performHandDetection)
        {
            findHands(frame);
        }
        
        display(frame);
    }

    void FindEyes(Mat frame)
    {
        // use imported leftEyeCascade to find eyes[]
        var eyes = leftEyeCascade.DetectMultiScale(frame, 1.1, 2, HaarDetectionType.ScaleImage);

        // if there are at least 2 eyes detected
        if (eyes.Length >= 2)
        {
            Eye1 = eyes[0];
            Eye2 = eyes[1];
            
            // get locations of eyes
            eye1X = eyes[0].Location.X;
            eye1Y = eyes[0].Location.Y;
            eye2X = eyes[1].Location.X;
            eye2Y = eyes[1].Location.Y;
        }
    }
    void findNewFace(Mat frame)
    {
        // uses imported faceCascade to find face(s)
        var faces = faceCascade.DetectMultiScale(frame, 1.1, 2, HaarDetectionType.ScaleImage);

        // if face is detected
        if (faces.Length >= 1)
        {
            MyFace = faces[0];

            // save to cubeLocations to render
            cubeLocationX = MyFace.Location.X;
            cubeLocationY = MyFace.Location.Y;
        }
    }

    void findHands(Mat frame)
    {
        var hands = handCascade.DetectMultiScale(frame, 1.1, 2, HaarDetectionType.ScaleImage);

        if (hands.Length >= 1)
        {
            Hand1 = hands[0];
            hand1X = hands[0].Location.X;
            hand1Y = hands[0].Location.Y;
        }
    }


    void DisplayHand(Mat frame)
    {
        //calculate distance between face and hand
        var distance = Math.Sqrt((Math.Pow(hand1X - cubeLocationX, 2) + Math.Pow(hand1Y - cubeLocationY, 2)));

        float handFromFaceDeadZone = 200;

        if (distance >= handFromFaceDeadZone)
        {
            frame.Rectangle(Hand1, new Scalar(225, 0, 0), 2);
        }
    }

    void display(Mat frame)
    {
        // if face + eyes are found
        if (MyFace != null && (Eye1 != null || Eye2 != null))
        {
            var myFaceArea = MyFace.Size.Width * MyFace.Size.Height;
            var eyesTotalArea = Eye1.Size.Width * Eye1.Size.Height + Eye2.Size.Width * Eye2.Size.Height;

            bool faceSizeValid = myFaceArea > eyesTotalArea;
            bool eyesNotTooClose = Eye1.Intersect(Eye2).Size.Height * Eye1.Intersect(Eye2).Size.Width < 10;
            bool eyesWithinFace = MyFace.Intersect(Eye1).Size.Height * MyFace.Intersect(Eye1).Size.Width > 0 && MyFace.Intersect(Eye2).Size.Height * MyFace.Intersect(Eye2).Size.Width > 0;
            bool notExtremeEyeMovement = Math.Abs(eye1Y - eye2Y) < Eye1.Size.Height;

            // checks the validity of the face detection and if the system is working correctly
            if (faceSizeValid && eyesNotTooClose && eyesWithinFace && notExtremeEyeMovement)
            {

                float eyeHeightDifference = 0f;

                // gets the height difference of the 2 eyes
                if (eye1X < eye2X)
                {
                    eyeHeightDifference = eye1Y - eye2Y;
                } 
                else if (eye2X < eye1X)
                {
                    eyeHeightDifference = eye2Y - eye1Y;
                }

                // draws the green rectangles on the webcam frame for visualization
                frame.Rectangle(MyFace, new Scalar(0, 0, 255), 2);
                frame.Rectangle(Eye1, new Scalar(0, 255, 0), 2);
                frame.Rectangle(Eye2, new Scalar(0, 255, 0), 2);

                // move the camera in accordance with the player eye difference
                translateCamera(eyeHeightDifference);
            }

            if (performHandDetection)
                DisplayHand(frame);

            // Flip() is there to make sure the user sees the right image
            Texture newtexture = OpenCvSharp.Unity.MatToTexture(frame.Flip(FlipMode.Y));

            float pipScale = .4f;

            cameraImage.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(-newtexture.width*pipScale * .7f, newtexture.height*pipScale*.7f, 0);
            cameraImage.GetComponent<RectTransform>().sizeDelta = new Vector2(newtexture.width*pipScale, newtexture.height*pipScale);
            cameraImage.GetComponent<CanvasRenderer>().SetTexture(newtexture);

            // check if screen was assigned in inspector
            if (screen)
            {
                Texture textureForScreen = OpenCvSharp.Unity.MatToTexture(frame.Flip(FlipMode.X));
                screen.GetComponent<Renderer>().material.mainTexture = newtexture;

            }   
        }
    }

    void translateCamera(float rotate)
    {
        var myFaceArea = MyFace.Size.Height * MyFace.Size.Width;

        // if the face area takes up less than this portion of the webcam fov
        //if (myFaceArea < (0.35 * _webCamTexture.height) * (0.25 * _webCamTexture.width))
        if (myFaceArea < (0.3 * _webCamTexture.height) * (0.2 * _webCamTexture.width))
        {
            // move player back
            playerCamera.transform.Translate(Vector3.back * Time.deltaTime * speed);
        }

        // if the face area takes up more than this portion of the webcam fov
        //if (myFaceArea > (0.45*_webCamTexture.height)*(0.3*_webCamTexture.width))
        if (myFaceArea > (0.40 * _webCamTexture.height) * (0.25 * _webCamTexture.width))
        {
            // move player forward
            playerCamera.transform.Translate(Vector3.forward * Time.deltaTime * speed);
        }

        // small dead zone
        if (Math.Abs(rotate / MyFace.Size.Height) > 0.06f)
        {
            playerCamera.transform.Rotate(Vector3.up, rotate / MyFace.Size.Height * Time.deltaTime * 500f);
        }

    }
}

