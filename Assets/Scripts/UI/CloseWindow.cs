using UnityEngine;

public class CloseWindow : MonoBehaviour
{
    public GameObject closeObject;

    public  void Open()    
    {
        closeObject.SetActive(true);
    }

    public void Close()
    {
        closeObject.SetActive(false);

    }

}
