using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

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

#if !UNITY_EDITOR
        bool initRes = InitMessageListener();
        if (!initRes)
        {
            Debug.LogError("Failed to initialize message listener, game is not running in a browser, terminating");
            Time.timeScale = 0;
            return;
        }

        OutBrowserMessage message = new() { action = "ready", args = null };
        string jsonMessage = JsonUtility.ToJson(message);
        SendToJavaScript(message);

        SendToJavaScript(new OutBrowserMessage
        {
            action = "setData",
            args = new Dictionary<string, object>
            {
            { "variableName", "test" },
            { "variableValue", 42 },
            { "variableType", "int" }
            }
        });
#endif
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
        Debug.Log("UNITY - Received message from JavaScript: " + message.action);

        switch (message.action)
        {
            case "forward":
                Player player = Object.FindFirstObjectByType<Player>();

                int distance = 1;
                try
                {
                    distance = int.Parse(message.args["distance"].ToString());
                }
                catch (KeyNotFoundException e)
                {
                    Debug.LogError("Failed to parse distance: " + e.Message);
                }

                float speed = 1f;
                try
                {
                    if (message.args["speed"] != null)
                    {
                        speed = float.Parse(message.args["speed"].ToString());
                    }
                }
                catch (KeyNotFoundException e)
                {
                    Debug.LogError("Failed to parse speed: " + e.Message);
                }

                if (player != null) player.EnqueueAction(new MovementAction(distance, speed));
                break;

            case "turn_left":
                player = Object.FindFirstObjectByType<Player>();
                if (player != null) player.EnqueueAction(new RotationAction("left"));
                break;

            case "turn_right":
                player = Object.FindFirstObjectByType<Player>();
                if (player != null) player.EnqueueAction(new RotationAction("right"));
                break;

            case "setLevel":
                int level = -1;
                try
                {
                    level = int.Parse(message.args["levelId"].ToString());
                }
                catch (KeyNotFoundException e)
                {
                    Debug.LogError("Failed to parse level: " + e.Message);
                }

                SceneManager.LoadScene(level.ToString());
                break;

            case "restartLevel":
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                break;

            default:
                Debug.LogError("Unknown action: " + message.action);
                break;
        }
    }
}