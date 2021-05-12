using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Notifications.Android;
using UnityEngine.Networking;
using LitJson;

public class SceneManage : MonoBehaviour
{
    public Text timeText;
    public Text currPrice;
    public Text msgText;
    public GameObject msgTextObj;

    public InputField serverAddr;
    public InputField requestData;

    public InputField usdtPriceInput;

    public InputField maxPrice;
    public InputField minPrice;

    public bool isCheck = false;

    private Coroutine checkCoroutine;


    void Start()
    {
        Debug.developerConsoleVisible = true;
        serverAddr.text = "https://api.binance.com/api/v3/ticker/price";
        requestData.text = "?symbol=ETHUSDT";
        usdtPriceInput.text = "6.48";
        maxPrice.text = "28000";
        minPrice.text = "2.8";

        msgText.text = "msg:点击Start开始监测";
        StartCoroutine(ShowTime());
    }
    private IEnumerator ShowTime()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f);
            timeText.text = System.DateTime.Now.ToLocalTime().ToString();
        }
    }
    public IEnumerator CheckData()
    {
        // 接受数据
        string recData = "";
        while (true)
        {
            // 从服务器接口获取数据
            using (UnityWebRequest uwr = UnityWebRequest.Get(serverAddr.text + requestData.text))
            {
                yield return uwr.SendWebRequest();
                if (uwr == null || uwr.isNetworkError || uwr.isHttpError)
                {
                    print("网络或请求错误");
                    msgText.text = "网络或请求错误";
                }else
                {
                    msgText.text = "请求正常";
                    recData = uwr.downloadHandler.text;
                    msgText.text = "数据：" + recData;
                    // 解析数据
                    JsonData data = JsonMapper.ToObject(recData);
                    msgText.text = "解析：" + data[1].ToString();
                    // 数据条件判断
                    double price = System.Convert.ToDouble(data[1].ToString());
                    currPrice.text = (price * System.Convert.ToDouble(usdtPriceInput.text)).ToString();
                    if (price * System.Convert.ToDouble(usdtPriceInput.text) >= System.Convert.ToDouble(maxPrice.text))
                    {
                        // 发送大于通知
                        NotificationSender.SendNotify(
                            "Max",
                            requestData.text + ": " + (price * System.Convert.ToDouble(usdtPriceInput.text)).ToString()
                            );
                        yield return new WaitForSeconds(30);
                    }else if (price * System.Convert.ToDouble(usdtPriceInput.text) <= System.Convert.ToDouble(minPrice.text))
                    {
                        // 发送小于通知
                        NotificationSender.SendNotify(
                            "Min",
                            requestData.text + ": " + (price * System.Convert.ToDouble(usdtPriceInput.text)).ToString()
                            );
                        yield return new WaitForSeconds(30);
                    }
                }
            }
            yield return new WaitForSeconds(1);
        }
    }

    // 点击发送消息
    public void StartEndBtn(GameObject btn)
    {
        if (isCheck)
        {
            isCheck = false;
            ActiveInput(true);
            msgText.text = "msg:点击Start开始监测";
            // 在监听，关闭监听
            if (checkCoroutine != null)
                StopCoroutine(checkCoroutine);
            btn.GetComponent<Text>().text = "Start";
        }else
        {
            isCheck = true;
            ActiveInput(false);
            msgText.text = "msg：开始监测";
            // 没在监听，打开监听
            checkCoroutine = StartCoroutine(CheckData());
            btn.GetComponent<Text>().text = "Detection…";
        }
    }
    public void ActiveInput(bool active)
    {
        serverAddr.interactable = active;
        requestData.interactable = active;
        usdtPriceInput.interactable = active;
        maxPrice.interactable = active;
        minPrice.interactable = active;
    }
    public void SwitchMSGActive()
    {
        if (msgTextObj.activeSelf)
            msgTextObj.SetActive(false);
        else
            msgTextObj.SetActive(true);
    }
    public void OpenSmalBoxURL()
    {
        Application.OpenURL("https://smalbox.top");
    }
}
