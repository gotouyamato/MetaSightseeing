using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System;
using UnityEditor;
using TMPro;
using UnityEngine.SceneManagement;
public class ToTexture : MonoBehaviour
{
    TextAsset csvFile; // CSVファイル
    List<string[]> csvDatas = new List<string[]>(); // CSVの中身を入れるリスト;
    List<UnityEngine.Texture2D> tx = new List<UnityEngine.Texture2D>(); // Textureを入れるリスト;
    List<double> xls = new List<double>();
    List<double> yls = new List<double>();
    public Texture2D texture;
    public Material TargetMaterial;
    double x;
    double y;
    public double x_max;
    public double x_min;
    public double y_max;
    public double y_min;
    Dictionary<int, double> dic = new Dictionary<int, double>();
    int Imagenum;

    [SerializeField]
    private Transform cameraRigTransform; //cameraRig
    // [SerializeField]
    // private OVRCameraRig cameraRigTransform;
    [SerializeField]
    private GameObject Image001; //mainのスフィア
    [SerializeField]
    private Canvas canvas; //canvas
    [SerializeField]
    private GameObject PositionImage; //位置座標を表示するimg
    [SerializeField]
    private TextMeshProUGUI Rigposi; //HMDのx座標
    [SerializeField]
    private TextMeshProUGUI Rigposi2; //HMDのy座標
    [SerializeField]
    private TextMeshProUGUI Rigposi3; //HMDのz座標
    [SerializeField]
    private GameObject MenuImage; //Menu画面のimg
    private GameObject icon;
    private GameObject icon2;
    private GameObject icon3;

    // private Transform cameraRigTransform;
    Vector3 Rigposition;
    Vector3 Image001posi;
    Vector3 iconposi;
    Vector3 canvasposi;
    Vector2 primaryAxis = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);


    // public OVRPlayerController playerController;


    string mapname;
    void Start()
    {
        Imagenum = 10146;
        mapname = "TudoiBashi";
        SetTexture(mapname, Imagenum); //mainの球体の中に360度画像のテクスチャを表示する

        Rigposition = cameraRigTransform.transform.localPosition; //HMDの位置座標を取得
        Rigposition.z = OVRManager.profile.eyeDepth; //HMDの位置座標を取得
        cameraRigTransform.transform.localPosition = Rigposition; //HMDの位置座標を取得
        Image001posi = Image001.transform.localPosition; //球体の位置をHMDの位置に合わせる
        icon = GameObject.Find("OVRCameraRig/TrackingSpace/CenterEyeAnchor/Canvas/MenuImage/icon");
        icon2 = GameObject.Find("OVRCameraRig/TrackingSpace/CenterEyeAnchor/Canvas/MenuImage/icon2");
        icon3 = GameObject.Find("OVRCameraRig/TrackingSpace/CenterEyeAnchor/Canvas/MenuImage/icon3");
    }

    void Update()
    {

        Rigposition = cameraRigTransform.transform.localPosition;
        cameraRigTransform.transform.localPosition = Rigposition; //位置座標をリアルタイムで更新

        //今回は実際の移動距離の2倍、座標が移動するように設定
        Rigposition.x = -Rigposition.x;
        Rigposition.x = Rigposition.x * 2;
        Rigposition.y = Rigposition.y * 2;
        Rigposition.z = Rigposition.z * 2;


        x = Rigposition.x;
        y = Rigposition.z;
        Image001posi.x = Rigposition.x;
        Image001posi.y = Rigposition.y;
        Image001posi.z = Rigposition.z;

        Debug.Log("カメラリグ" + Rigposition);
        Rigposi.text = Convert.ToString(Rigposition.x); //画面に位置座標を出力
        Rigposi2.text = Convert.ToString(Rigposition.z);
        Rigposi3.text = Convert.ToString(Rigposition.y);

        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D) || primaryAxis.x > 0.0f)
        {
            Rigposition.x = Rigposition.x - 0.015f;
            double x_posi = x - 0.015;
            // x = System.Math.Max(x_posi, x_min);
            x = System.Math.Max(Rigposition.x, x_min);
            Debug.Log("カメラリグ" + Rigposition);

        }
        else if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W) || primaryAxis.y > 0.0f)
        {
            Rigposition.z = Rigposition.z + 0.015f;
            double y_posi = y + 0.015;
            // y = System.Math.Min(y_posi, y_max);
            y = System.Math.Min(Rigposition.z, y_max);
            Debug.Log("カメラリグ" + Rigposition);
        }
        else if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A) || primaryAxis.x < 0.0f)
        {
            Rigposition.x = Rigposition.x + 0.015f;
            double x_posi = x + 0.015;
            // x = System.Math.Min(x_posi, x_max);
            x = System.Math.Min(Rigposition.x, x_max);
            Debug.Log("カメラリグ" + Rigposition);
        }
        else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S) || primaryAxis.y < 0.0f)
        {
            Rigposition.z = Rigposition.z - 0.015f;
            double y_posi = y - 0.015;
            // y = System.Math.Max(y_posi, y_min);
            y = System.Math.Max(Rigposition.z, y_min);
            Debug.Log("カメラリグ" + Rigposition);
        }

        ChangeMaterial(); //座標から画像の距離を求めてリアルタイムでマテリアルを変更


        //以下Menu操作
        // if (Input.GetKeyDown(KeyCode.Space) && MenuImage.activeSelf == false) //キーボード操作
        if (OVRInput.GetDown(OVRInput.RawButton.X) && MenuImage.activeSelf == false) //左コントローラーのXボタンが押された時の処理
        {
            MenuImage.SetActive(true);
        }
        // else if (Input.GetKeyDown(KeyCode.Space) && MenuImage.activeSelf == true) //キーボード操作
        else if (OVRInput.GetDown(OVRInput.RawButton.X) && MenuImage.activeSelf == true)
        {
            MenuImage.SetActive(false);
        }

        if (MenuImage.activeSelf == true) //Menu操作
        {
            // if (Input.GetKeyDown(KeyCode.DownArrow) && icon.activeSelf == true) //キーボード操作
            if (OVRInput.GetDown(OVRInput.RawButton.LThumbstickDown) && icon.activeSelf == true) //スティック下入力
            {
                icon.SetActive(false);
                icon2.SetActive(true);
            }
            // else if (Input.GetKeyDown(KeyCode.DownArrow) && icon2.activeSelf == true) //キーボード操作
            else if (OVRInput.GetDown(OVRInput.RawButton.LThumbstickDown) && icon2.activeSelf == true) //スティック下入力
            {
                icon2.SetActive(false);
                icon3.SetActive(true);
            }
            // if (Input.GetKeyDown(KeyCode.UpArrow) && icon2.activeSelf == true) //キーボード操作
            if (OVRInput.GetDown(OVRInput.RawButton.LThumbstickUp) && icon2.activeSelf == true) //スティック上入力
            {
                icon.SetActive(true);
                icon2.SetActive(false);
            }
            // else if (Input.GetKeyDown(KeyCode.UpArrow) && icon3.activeSelf == true) //キーボード操作
            else if (OVRInput.GetDown(OVRInput.RawButton.LThumbstickUp) && icon3.activeSelf == true) //スティック上入力
            {
                icon2.SetActive(true);
                icon3.SetActive(false);
            }


            // if (Input.GetKeyDown(KeyCode.Return) && icon.activeSelf == true) //キーボード操作
            if (OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger) && icon.activeSelf == true)
            {
                ResetAllValues(); //SetTextureで設定した値を初期化
                Imagenum = 10146;
                mapname = "TudoiBashi";
                SetTexture(mapname, Imagenum);
                MenuImage.SetActive(false);

            }
            // else if (Input.GetKeyDown(KeyCode.Return) && icon2.activeSelf == true) //キーボード操作
            else if (OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger) && icon2.activeSelf == true)
            {
                ResetAllValues(); //SetTextureで設定した値を初期化
                // Imagenum = 101;
                // mapname = "MapB";
                // SetTexture(mapname, Imagenum);
                SceneManager.LoadScene("StreetView");
                MenuImage.SetActive(false);
            }
            // else if (Input.GetKeyDown(KeyCode.Return) && icon3.activeSelf == true) //キーボード操作
            else if (OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger) && icon3.activeSelf == true)
            {
                ResetAllValues(); //SetTextureで設定した値を初期化
                // Imagenum = 101;
                // mapname = "MapC";
                // SetTexture(mapname, Imagenum);
                // SceneManager.LoadScene("GoogleEarth");
                Imagenum = 10146;
                mapname = "TudoiBashi";
                SetTexture(mapname, Imagenum);
                MenuImage.SetActive(false);
            }

        }
        if (OVRInput.GetDown(OVRInput.RawButton.A) && PositionImage.activeSelf == true) //カメラリグのポジションを非表示
        {
            PositionImage.SetActive(false);
            Rigposi.enabled = !Rigposi.enabled;
            Rigposi2.enabled = !Rigposi2.enabled;
            Rigposi3.enabled = !Rigposi3.enabled;

        }
        else if (OVRInput.GetDown(OVRInput.RawButton.A) && PositionImage.activeSelf == false) //カメラリグのポジションを表示
        {
            PositionImage.SetActive(true);
            Rigposi.enabled = !Rigposi.enabled;
            Rigposi2.enabled = !Rigposi2.enabled;
            Rigposi3.enabled = !Rigposi3.enabled;

        }
    }



    void SetTexture(string map, int Imagenum) //360度画像パスと、座標を設定したCSVを読み込む
    {

        if (map == "TudoiBashi") //map名によるテクスチャの変更
        {
            csvFile = Resources.Load("ImageCsv2") as TextAsset; // Resouces下のCSV読み込み
        }
        else if (map == "MapB")
        {
            csvFile = Resources.Load("ImageCsv3") as TextAsset; // Resouces下のCSV読み込み
        }
        else if (map == "MapC")
        {
            csvFile = Resources.Load("ImageCsv4") as TextAsset; // Resouces下のCSV読み込み
        }

        StringReader reader = new StringReader(csvFile.text); // , で分割しつつ一行ずつ読み込みリストに追加していく

        while (reader.Peek() != -1) // reader.Peaekが-1になるまで
        {
            string line = reader.ReadLine(); // 一行ずつ読み込み
            csvDatas.Add(line.Split(',')); // , 区切りでリストに追加
        }

        int csvCount = csvDatas.Count;
        for (int i = 0; i < csvCount; i++)
        {
            double x = double.Parse(csvDatas[i][1]);
            double y = double.Parse(csvDatas[i][2]);
            string path = csvDatas[i][3];
            string new_path = "Images/" + map + "/R00" + Imagenum.ToString();
            Imagenum = Imagenum + 1;
            Texture2D new_tex = Resources.Load<Texture2D>(new_path);
            tx.Add(new_tex); //テクスチャをtxに追加
            xls.Add(x);
            yls.Add(y);
        }
        x_max = xls.Max();
        x_min = xls.Min();
        y_max = xls.Max();
        y_min = xls.Min();

    }
    void ResetAllValues() //SetTextureで設定した値を初期化する関数
    {
        csvFile = null;
        csvDatas.Clear();
        tx.Clear();
        xls.Clear();
        yls.Clear();

        x_max = 0.0f;
        x_min = 0.0f;
        y_max = 0.0f;
        y_min = 0.0f;

        Imagenum = 0;
        Rigposition = new Vector3(0, 0, 0);
        Rigposi.text = Convert.ToString(Rigposition.x);
        Rigposi2.text = Convert.ToString(Rigposition.z);
        Rigposi3.text = Convert.ToString(Rigposition.y);
    }
    void ChangeMaterial() //座標から画像の距離を求めてマテリアルを変更
    {
        double dis;
        int x_Count = xls.Count;
        dic.Clear();
        for (int j = 0; j < x_Count; j++)
        {
            dis = (x - xls[j]) * (x - xls[j]) + (y - yls[j]) * (y - yls[j]); //距離を計算
            dic.Add(j, dis);
        }
        double minValue = dic.Values.Min(); //計算結果の最小値
        int k1 = dic.FirstOrDefault(x => x.Value.Equals(minValue)).Key;
        // //マテリアルを変更
        TargetMaterial.SetTexture("_MainTex", tx[k1]);
    }
}