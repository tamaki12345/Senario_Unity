using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine;

public class SaveData{
    public string fileName;
    public int pointer;
    public string name;
    public string imageAdr;
    public string backGroundAdr;
    public bool reading;

    public SaveData(string fileName, int pointer, string name, string imageAdr, string backGroundAdr, bool reading){
        this.fileName = fileName;
        this.pointer = pointer;
        this.name = name;
        this.imageAdr = imageAdr;
        this.backGroundAdr = backGroundAdr;
        this.reading = reading;
    }

    public SaveData(){
        fileName="test";
        pointer=0;
        name="";
        imageAdr="";
        backGroundAdr="";
        reading=false;
    }

    public string Tostring(){
        return fileName + "\n" + pointer + "\n" + name + "\n" + imageAdr + "\n" + backGroundAdr + "\n" + reading;
    }
}

public class StartView : MonoBehaviour
{
    private string saveAdr="Assets/Script/saveData.json";
    private SaveData saveData;

    public void StartGame(){
        textViewer.continueGame=false;
        textViewer.adr="test";
        SceneManager.LoadScene("MainView");
    }

    public void ContinueGame(){
        GetSaveData();
    }

    private void GetSaveData(){
        Addressables.LoadAssetAsync<TextAsset>(saveAdr).Completed += msg =>//load save data from json
        {
            if(msg.Result==null){
                Debug.Log("File Error");
                #if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
                #else
                    Application.Quit();
                #endif
                return;
            }
            else{
                saveData=JsonUtility.FromJson<SaveData>(msg.Result.ToString());
                textViewer.saveData=saveData;
                textViewer.continueGame=true;
                textViewer.adr=saveData.fileName;
                SceneManager.LoadSceneAsync("MainView");
            }
        };
    }
}
