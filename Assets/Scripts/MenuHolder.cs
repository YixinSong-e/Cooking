﻿using LitJson;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

public class MenuHolder: Place {

    [SerializeField] private CookingStep stepPrefab = null;
    List<CookingStep> steps;
    GameController gameController;
    private Dragable drag;

    private Vector2 unitSize; // 在菜单栏里的步骤的大小

    void Start() {
        gameController = FindObjectOfType<GameController>();
        InitializedMenu("Jsons/test"); // Resources文件夹下读取文件不用后缀名
        unitSize = stepPrefab.GetComponent<RectTransform>().sizeDelta;
    }

    void Update() {
        
    }

    public override void DragEffectBegin(Dragable d) {
        drag = d;
    }

    public override void DragEffectEndIn() {

        CookingStep addStep = drag.GetComponent<CookingStep>(); // 被拖的步骤
        /* foreach(var step in addStep.Control) // 依赖addStep且在时间条上的自动弹回菜单栏
        {
            if(!step.DependNotSatisfied.Exists(t => t.name == addStep.name))
            {
                step.DependNotSatisfied.Add(addStep);
                step.transform.SetParent(transform);
                step.GetComponent<Dragable>().SetDragSize(unitSize);
                step.canDrag = false;
                step.Belong = null;
                step.GetComponent<Dragable>().ImageChange();
            }
        } */
        drag.transform.SetParent(transform);
        drag.GetComponent<CookingStep>().Belong = null;
        drag.SetDragSize(unitSize);
        drag = null;
    }

    public override void DragEffectEndOut() {
        drag = null;
    }

    private void InitializedMenu(string filename)
    {
        //StreamReader streamreader = new StreamReader(Application.dataPath + filename);
        //JsonReader jr = new JsonReader(streamreader);
        //JsonData jd = JsonMapper.ToObject(jr);
        JsonData jd = JsonMapper.ToObject(Resources.Load<TextAsset>(filename).text);
        gameController.dishinfo = (string)jd["简介"];
        gameController.dishinst = (string)jd["详介"];
        foreach (JsonData i in jd["步骤"])
        {
            CookingStep cs = Instantiate(stepPrefab, transform);
            cs.Init((string)i["名字"], (int)i["持续时间"], (bool)i["能否同时"],(string)i["图片"], (int)i["台子"]);
            var drag = cs.GetComponent<Dragable>();
            gameController.stepCollection.CookingSteps.Add(cs);
        }
        for (int i = 0; i < jd["步骤"].Count; i++)
        {
            CookingStep cs = gameController.stepCollection.CookingSteps[i];
            string path = "Images/步骤/" + cs.spritePath;
            Sprite sprite = Resources.Load<Sprite>(path);
            Image t = cs.GetComponentsInChildren<Image>()[2];
            t.sprite = sprite; t.preserveAspect = true;
            Text timeText = cs.GetComponentsInChildren<Text>().Where(x => x.name == "Time").First();
            timeText.text = cs.Time.ToString() + " min";
            Text nameText = cs.GetComponentsInChildren<Text>().Where(x => x.name == "Name").First();
            nameText.text = cs.Name;
            JsonData depend = jd["步骤"][i]["前置条件"];
            foreach (JsonData j in depend) cs.DirectDepend.Add(gameController.stepCollection.FindByName((string)j));
        }
        gameController.stepCollection.CalcDepend();        
        gameController.stepCollection.CookingSteps.ForEach((x) =>
            x.Depend.ForEach((y) =>
                x.DependNotSatisfied.Add(y)
            )
        );

        /* foreach(var step in gameController.stepCollection.CookingSteps)
        {
            if (step.DependNotSatisfied.Count > 0)
            {
                step.canDrag = false;
            }
        } */
    }


    public static List<T> Clone<T>(object List)
    {
        using (Stream objectStream = new MemoryStream())
        {
            var formatter = new BinaryFormatter();
            formatter.Serialize(objectStream, List);
            objectStream.Seek(0, SeekOrigin.Begin);
            return formatter.Deserialize(objectStream) as List<T>;
        }
    }

}
