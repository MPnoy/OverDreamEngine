//using System;
//using System.Collections.Generic;
//using UnityEngine;

//[Serializable]
//public class PreviewObj
//{
//    public Texture2D t;
//    public string path = "";
//    public bool isImage = true;

//    public PreviewObj(string path, bool isImage)
//    {
//        this.path = path;
//        this.isImage = isImage;
//        if (isImage)
//        {
//            var tmp = (Texture2D)Resources.Load(path);
//            var rt = new RenderTexture(1920 / 4, 1080 / 4, 0, RenderTextureFormat.ARGB32);
//            rt.Create();
//            Graphics.Blit(tmp, rt);
//            t = TextureHelper.ToTexture2D(rt);
//            Resources.UnloadAsset(tmp);
//        }

//    }
//}

//public class GalleryHolder : MonoBehaviour
//{
//    public bool IsTest = false;

//    public string[] pathsBG;
//    public string[] pathsCG;
//    public string[] pathsMU;
//    public List<PreviewObj> bgList;
//    public List<PreviewObj> cgList;
//    public List<PreviewObj> muList;
//    public Galler_Wind gal;

//    void Start()
//    {
//        List<PreviewObj> prevList = new List<PreviewObj>();
//        for (int i = 0; i < pathsBG.Length; i++)
//        {
//            prevList.Add(new PreviewObj(pathsBG[i], true));
//        }
//        bgList = prevList;

//        prevList = new List<PreviewObj>();
//        for (int i = 0; i < pathsCG.Length; i++)
//        {
//            prevList.Add(new PreviewObj(pathsCG[i], true));
//        }
//        cgList = prevList;
//        prevList = new List<PreviewObj>();
//        for (int i = 0; i < pathsMU.Length; i++)
//        {
//            prevList.Add(new PreviewObj(pathsMU[i], false));
//        }
//        muList = prevList;

//        if (IsTest)
//        {
//            gal.bgList = bgList;
//            gal.cgList = cgList;
//            gal.muList = muList;
//            gal.SetMode(Galler_Wind.GallerMode.BG);
//        }
//    }

//    public void PushInGal()
//    {
//        gal = gameObject.GetComponentInChildren<Galler_Wind>();
//        gal.bgList = bgList;
//        gal.cgList = cgList;
//        gal.muList = muList;
//        gal.SetMode(Galler_Wind.GallerMode.BG);
//    }

//}
