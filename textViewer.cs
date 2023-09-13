using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using System.IO;
using TMPro;

public class textViewer : MonoBehaviour
{
    public static SaveData saveData;// = new SaveData();
    public static bool continueGame=false;
    public static string adr="test"; //dataFile address
    private static string saveAdr="Assets/Script/saveData.json";//save dat address
    private string data, currentImg, backgourndImg; //dataFile, current image name, background image name
    private TextMeshProUGUI textUI, nameUI; 
    private TextMeshProRuby textUIRuby;
    private Image img, newImg, background; //character iamge, next image, backgournd image
    private AsyncOperationHandle<Sprite> spriteHandle;
    private int pointer = 0, savePointer; //data file pointer
    private Button controller; //UI as button
    private bool reading=false, viewing=false, skip=false, choose=false, option=false, end=false, clickable=false; //reading serif, making text UI, skip making text UI, choosing choices, open option, end game, can click to skip
    private GameObject[] choices = new GameObject[3];
    private TextMeshProUGUI[] choiceTexts = new TextMeshProUGUI[3];
    private string[] choiceAddress = new string[3];//choice transition address
    private GameObject options, nameUIObj;//option UI

    void Start(){
        controller = GameObject.Find("Controller").gameObject.GetComponent<Button>();
        options=GameObject.Find("Option").gameObject;//Option
        options.SetActive(false);
        img=GameObject.Find("Image").gameObject.GetComponent<Image>();//charaqcter image
        img.enabled=false;
        background=GameObject.Find("BackGround").gameObject.GetComponent<Image>();//backgournd image
        background.enabled=false;
        textUI=GameObject.Find("Text").gameObject.GetComponent<TextMeshProUGUI>();//text
        textUI.enabled=false;
        textUIRuby=GameObject.Find("Text").gameObject.GetComponent<TextMeshProRuby>();//text
        nameUIObj=GameObject.Find("NameUI").gameObject;
        nameUI=GameObject.Find("Name").gameObject.GetComponent<TextMeshProUGUI>();//name
        nameUI.enabled=false;
        nameUIObj.SetActive(false);
        Application.targetFrameRate = 120;
        controller.onClick.AddListener(() => {
            if(end) Finish();
            if(clickable){
                if(!choose){
                    if(!viewing){//while not making text UI
                        if(!reading){//while not reading serif
                            clickable=false;
                            readText(0,0);
                        }
                        else{//reading serif
                            textUIRuby.Text="";
                            readText(2,0);
                        }
                    }
                    else{
                        skip=true;//skip making textUI
                    }
                }
            }
        });

        Addressables.LoadAssetAsync<TextAsset>(adr).Completed += msg =>//load textfile as data
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
                data=msg.Result.ToString();

                if(continueGame){
                    adr=saveData.fileName;
                    pointer=saveData.pointer;
                    reading=saveData.reading;
                    continueGame=false;
                    Addressables.LoadAssetAsync<Sprite>(saveData.imageAdr).Completed += handle=>{
                        spriteHandle=handle;
                        if(handle.Result==null){//if new file is null
                            reading=false;
                            return;
                        }
                        else{
                            img.sprite=handle.Result;
                        }
                    };
                    Addressables.LoadAssetAsync<Sprite>(saveData.backGroundAdr).Completed += handle=>{
                        spriteHandle=handle;
                        if(handle.Result==null){//if new file is null
                            reading=false;
                            return;
                        }
                        else{
                            background.sprite=handle.Result;
                            currentImg=saveData.backGroundAdr;
                        }
                    };
                    StartCoroutine(DelayCoroutine(1,() => {
                        img.enabled=true;
                        background.enabled=true;
                        textUI.enabled=true;
                        nameUI.enabled=true;
                        nameUIObj.SetActive(true);
                        textUI.text="";
                        nameUI.text=saveData.name;
                        readText(2,0);
                    }));
                }
                else{
                    StartCoroutine(DelayCoroutine(1,() => {
                        img.enabled=true;
                        background.enabled=true;
                        textUI.enabled=true;
                        nameUI.enabled=true;
                        nameUIObj.SetActive(true);
                        SetUpScene();//set up scene (make background image)
                        readText(0,0);//read data and make UI(characer image and text)
                    }));
                }
            }
        };

        choices[0]=GameObject.Find("Choice1").gameObject;  //get choices and set unactive
        choices[1]=GameObject.Find("Choice2").gameObject;
        choices[2]=GameObject.Find("Choice3").gameObject;
        for(int i=0;i<3;i++){
            choiceTexts[i]=choices[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            choices[i].SetActive(false);
        }
    }

    void Update(){
        if(end){
            if(Input.GetKeyDown(KeyCode.Return)) Finish();
        }
        else if(clickable && !choose){
            if(Input.GetKeyDown(KeyCode.Return)){
                if(!viewing){//when not making text UI
                    if(!reading){//when not reading serif
                        clickable=false;
                        readText(0,0);
                    }
                    else{//reading serif
                        textUIRuby.Text="";
                        savePointer=pointer;
                        readText(2,0);
                    }
                }
                else{
                    skip=true;//skip making textUI
                }
            }
        }
        if(Input.GetKeyDown(KeyCode.Escape)){
            if(!option && !viewing){
                options.SetActive(true);
                option=true;
            } 
            else{
                options.SetActive(false);
                option=false;
            }
        }
    }

    void OnDestroy(){
        if(spriteHandle.IsValid()) Addressables.Release(spriteHandle);
    }

    private void SetUpScene(){//set up scene (make background image)
        char c; 
        string ret="";
        int i=pointer;

        if(data.Length>6){
            while(i<data.Length){
                c=data[i];
                if(c=='$'){ //when start loading on '$'
                    while(true){
                        i++;
                        c=data[i]; //read every one character
                        if(c=='$'){ //finish loading on '$'
                            i+=2;
                            break;
                        }
                        ret += c; //add character to string 
                    }
                    if(ret!=""){
                        Addressables.LoadAssetAsync<Sprite>(ret).Completed += handle =>{ //load background image on address 'ret'
                            spriteHandle = handle;
                            if(handle.Result==null){ // if load null
                                Debug.Log("File Error");
                                return;
                            }
                            else{
                                background.sprite=handle.Result;
                                backgourndImg=ret;
                            }
                        };
                        pointer=i+1; //increase pointer
                        break;
                    }
                    ret="";
                }
                else{ //if start not on '$'
                    Debug.Log("File Error");
                    break;
                }
                i++;
            }
        }
        else Debug.Log("File Error");
    }

    private void readText(int _mode, int _line){ //read data file 
        char c; 
        string ret="";
        int i=pointer, mode=_mode, line=_line;
        if(i>=data.Length-1){ //pointer is over file size
            return;
        }

        if(mode!=2 && data[i]=='$'){//not reading serif and start loading from '$'
            while(true){
                i++;
                c=data[i];
                if(c=='$'){// end on '$'
                    i+=2;
                    break;
                }
                ret += c;
            }
        }
        else if(mode!=2 && data[i]!='$') Debug.Log("File Error:mode "+mode);

        if(mode==0){ // mode loading character image
            pointer=i;
            pointer++;

            if(ret==""){
                textUIRuby.Text=""; //reset textUI
                img.enabled=true;
                readText(1,0);
                return;
            }
            
            else if(ret=="NONE"){
                img.enabled=false;//if load NONE, disenable image
                textUIRuby.Text=""; //reset textUI
                readText(1,0);
                return;
            }

            else if(ret=="background"){//reload background
                SetUpScene();
                readText(0,0);
                return;
            }

            else if(ret=="load"){//load new scene
                i++;
                ret="";

                if(data[i]!='$') Debug.Log("File Error");

                while(true){
                    i++;
                    c=data[i];
                    if(c=='$'){// end on '$'
                        i+=2;
                        break;
                    }
                    ret += c;
                }
                SceneTransition(ret);
                return;
            }

            else if(ret=="end"){// end of scenario
                end=true;
                return;
            }

            else if(ret=="choice"){ // strat choose choice
                textUIRuby.Text=""; //reset textUI
                choose=true;
                i+=3;
                for(int j=0;j<3;j++){
                    choices[j].SetActive(true);
                    ret="";
                    while(true){
                        i++;
                        c=data[i];
                        if(c=='$') break;
                        ret+=c;
                    }
                    choiceTexts[j].text=ret;
                    ret="";
                    //i++;
                    while(true){
                        i++;
                        c=data[i];
                        if(c=='\n') break;
                        ret+=c;
                    }
                    choiceAddress[j]=ret;
                }
                choices[0].GetComponent<Button>().onClick.AddListener(() => {
                    SceneTransition(choiceAddress[0].Substring(0,choiceAddress[0].Length-1));
                });
                choices[1].GetComponent<Button>().onClick.AddListener(() => {
                    SceneTransition(choiceAddress[1].Substring(0,choiceAddress[1].Length-1));
                });
                choices[2].GetComponent<Button>().onClick.AddListener(() => {
                    SceneTransition(choiceAddress[2].Substring(0,choiceAddress[2].Length-1));
                });
                return;
            }

            else if(ret!=""){//character image  file name is not empty and no same as current image
                textUIRuby.Text=""; //reset textUI
                img.enabled=true;
                Addressables.LoadAssetAsync<Sprite>(ret).Completed += handle=>{
                    spriteHandle=handle;
                    if(handle.Result==null){//if new file is null
                        reading=false;
                        return;
                    }
                    else{
                        img.sprite=handle.Result;
                        currentImg=ret;
                        StartCoroutine(DelayCoroutine(1,() => {
                            readText(1,0);//read data and load character name
                        }));
                        return;
                    }
                };
            }
        }
        else if(mode==1){ //mode read character name
            nameUI.text=ret;
            pointer=i+3; //increase pointer to skip "$\n$"
            savePointer=pointer;
            readText(2,0); // read serif
            return;
        }
        else if(mode==2){ //mode read serif (one by one)
            clickable=true;
            i++;
            viewing=true; //making textUI
            c=data[i];
            if(c!='$'){ //not end of serif
                if(c==';'){
                    string ruby="", serif="";
                    i++;
                    while(true){
                        c=data[i];
                        if(c==';'){
                            i++;
                            break;
                        }
                        ruby+=c;
                        i++;
                    }
                    while(true){
                        c=data[i];
                        if(c==';'){
                            i++;
                            break;
                        }
                        serif+=c;
                        i++;
                    }
                    pointer=i;
                    readRubySerif(ruby,serif,0,line);
                    return;
                }

                else if(c=='\n'){ //line changed
                    line++; //increase line number
                }
                if(line==3){ //if line number is max, stop making UI and return pointer
                    skip=false;
                    viewing=false;
                    int j=i;
                    if(data[j+1]!='$'){
                        reading=true;
                        pointer=i;
                    }
                    else{
                        reading=false;
                        pointer=i+4;
                    }
                    return;
                }
                int view=1;
                if(skip) view=0;//if skip is true, don't wait to show characters
                StartCoroutine(DelayCoroutine(view,() => {
                    textUIRuby.Text+=c;
                    pointer=i;
                    readText(2,line);// read next character
                }));
                return;
            }
            else{ //end of serif
                skip=false;
                reading=false;
                viewing=false;
                pointer=i+3;
                return;
            }
        }
    }

    private IEnumerator DelayCoroutine(int waitTime,Action action)//wait 0.1 * n [sec]
    {
        yield return new WaitForSeconds(0.1f*waitTime);
        action?.Invoke();
    }

    private void readRubySerif(string ruby, string serif, int idx, int line){
        if(idx==serif.Length){
            string tmp=textUIRuby.Text;
            tmp=tmp.Substring(0,tmp.Length-serif.Length);
            textUIRuby.Text=tmp+"<r="+ruby+">"+serif+"</r>";
            pointer--;
            readText(2,line);
            return;
        }
        int view=1;
        if(skip) view=0;//if skip is true, don't wait to show characters
        StartCoroutine(DelayCoroutine(view,() => {  
            textUIRuby.Text+=serif[idx];
            readRubySerif(ruby,serif,idx+1,line);
        }));
        return;
    }

    public void SceneTransition(string _adr){
        choose=false;
        textViewer.adr = _adr;
        pointer=0;
        for(int i=0;i<3;i++){
            choices[i].SetActive(false);
        }
        Addressables.LoadAssetAsync<TextAsset>(adr).Completed += msg =>//load textfile as data
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
                data=msg.Result.ToString();
                SetUpScene();//set up scene (make background image)
                readText(0,0);//read data and make UI(characer image and text)
            }
        };
    }

    public void Save(){
        saveData=new SaveData(adr, savePointer, nameUI.text, currentImg, backgourndImg, reading);

        string jsonData = JsonUtility.ToJson(saveData);
        StreamWriter writer = new StreamWriter(saveAdr, false);
        writer.Write(jsonData);
        writer.Flush();
        writer.Close();
        Finish();
    }

    public void Finish(){
        SceneManager.LoadScene("StartView");
    }
}