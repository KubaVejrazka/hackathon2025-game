using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class WebGLMessageHandler : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void SendMessageToJS(string message);

    [DllImport("__Internal")]
    private static extern bool InitMessageListener();

    [System.Serializable]
    public class OutBrowserMessage
    {
        public string action;
        public object args;
    }

    public class InBrowserMessage
    {
        public string action;
        public Dictionary<string, object> args;
    }

    private static WebGLMessageHandler instance;

    void Start()
    {
        Debug.Log("WebGLMessageHandler started");

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        bool initRes = InitMessageListener();
        if (!initRes)
        {
            Debug.LogError("Failed to initialize message listener, game is not running in a browser, terminating");
            Time.timeScale = 0;
            return;
        }
    }

    public void _ReceiveFromJavaScript(string jsonMessage)
    {
        Debug.Log("UNITY - Received raw message: " + jsonMessage);

        JObject jsonObject = JObject.Parse(jsonMessage);

        InBrowserMessage message = new InBrowserMessage
        {
            action = jsonObject["action"].ToString(),
            args = new Dictionary<string, object>(),
        };

        if (jsonObject["args"] != null)
        {
            JObject argsObject = (JObject)jsonObject["args"];
            foreach (var property in argsObject.Properties())
            {
                message.args[property.Name] = property.Value.ToString();
            }
        }

        ReceiveFromJavaScript(message);
    }

    public static void SendToJavaScript(OutBrowserMessage message)
    {
        Debug.Log("UNITY - Sending message to JavaScript: " + JsonConvert.SerializeObject(message));
        SendMessageToJS(JsonConvert.SerializeObject(message));
    }

    public static void ReceiveFromJavaScript(InBrowserMessage message)
    {
        //Debug.Log("UNITY - Received message from JavaScript: " + message.action);

        switch (message.action)
        {
            case "forward":
                Player player = Object.FindFirstObjectByType<Player>();

                int distance = 1;
                if (message.args["distance"] != null)
                {
                    distance = int.Parse(message.args["distance"].ToString());
                }
                
                float speed = 1f;
                if (message.args["speed"] != null)
                {
                    speed = float.Parse(message.args["speed"].ToString());
                }

                if (player != null) player.EnqueueMove(Vector3.forward, distance, speed);
                break;

            case "back":
                player = Object.FindFirstObjectByType<Player>();

                distance = 1;
                if (message.args["distance"] != null)
                {
                    distance = int.Parse(message.args["distance"].ToString());
                }

                speed = 1f;
                if (message.args["speed"] != null)
                {
                    speed = float.Parse(message.args["speed"].ToString());
                }

                if (player != null) player.EnqueueMove(Vector3.back, distance, speed);
                break;

            case "left":
                player = Object.FindFirstObjectByType<Player>();

                distance = 1;
                if (message.args["distance"] != null)
                {
                    distance = int.Parse(message.args["distance"].ToString());
                }

                speed = 1f;
                if (message.args["speed"] != null)
                {
                    speed = float.Parse(message.args["speed"].ToString());
                }

                if (player != null) player.EnqueueMove(Vector3.left, distance, speed);
                break;

            case "right":
                player = Object.FindFirstObjectByType<Player>();

                distance = 1;
                if (message.args["distance"] != null)
                {
                    distance = int.Parse(message.args["distance"].ToString());
                }

                speed = 1f;
                if (message.args["speed"] != null)
                {
                    speed = float.Parse(message.args["speed"].ToString());
                }

                if (player != null) player.EnqueueMove(Vector3.right, distance, speed);
                break;

            default:
                Debug.Log("UNITY - Received test message from JavaScript: " + message.args["test"]);
                break;
        }
    }
}