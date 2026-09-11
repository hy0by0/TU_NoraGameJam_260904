using UnityEngine;

public class AccessWeb : MonoBehaviour
{
    public string url; // URLを指定する変数
    public void clickWeb()
    {
        Application.OpenURL(url);
    }
}
