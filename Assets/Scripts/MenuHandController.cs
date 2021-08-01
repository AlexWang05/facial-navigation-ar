using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCvSharp;
using System;


public class MenuHandController : MonoBehaviour
{
    public UnityEngine.UI.Button topButton;
    public UnityEngine.UI.Button bottomButton;

    // first camera device in array
    public int cameraDevice = 0;
    private GameObject cameraImage;

    WebCamTexture _webCamTexture;

    // width and height at which the video is processed at
    private int videoWidth = 512;
    private int videoHeight = 288;

    public CascadeClassifier handCascade;

    // request fps
    private int videoFPS = 10;

    // location of hand
    private float hand1X;
    private float hand1Y;

    OpenCvSharp.Rect Hand1;

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

        // import haar cascade xml file
        handCascade = new CascadeClassifier("Assets/handcascade.xml");
    }

    // LateUpdate is used because update only needs to happen when frame is updated
    void LateUpdate()
    {
        // start making a new texture
        // converts webcam texture to Mat
        Mat frame = OpenCvSharp.Unity.TextureToMat(_webCamTexture);

        findHands(frame);

        DisplayHand(frame);


        // check height of hand
        if (hand1Y >= 180)
        {
            bottomButton.Select();
        }
        else
        {
            topButton.Select();
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
        frame.Rectangle(Hand1, new Scalar(225, 0, 0), 2);

        // Flip() is there to make sure the user sees the right image
        Texture newtexture = OpenCvSharp.Unity.MatToTexture(frame.Flip(FlipMode.Y));

        float pipScale = 0.4f;

        cameraImage.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(-newtexture.width * pipScale * .7f, newtexture.height * pipScale * .7f, 0);
        cameraImage.GetComponent<RectTransform>().sizeDelta = new Vector2(newtexture.width * pipScale, newtexture.height * pipScale);
        cameraImage.GetComponent<CanvasRenderer>().SetTexture(newtexture);
    }

}

