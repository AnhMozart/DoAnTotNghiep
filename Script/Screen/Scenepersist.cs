using UnityEngine;

public class Scenepersist : MonoBehaviour
{
    private void Awake()
    {
       int numScenePersist = FindObjectsByType<Scenepersist>(FindObjectsSortMode.None).Length;
        if(numScenePersist > 1)
        {
            Destroy(gameObject);

        }   
        else
        {
            DontDestroyOnLoad(gameObject);
        }    
    }

    public void ResetScenePersist()
    {
        Destroy(gameObject);
    }    
}
