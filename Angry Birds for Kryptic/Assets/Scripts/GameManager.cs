using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.UI;
public class GameManager : MonoBehaviour
{
    public static GameManager instance; //only do this if you will only ever have one copy of this script
    public int MaxNumberofShots = 3;
    [SerializeField] private float secondsToWaitBeforeDeathCheck = 3f;
    [SerializeField] GameObject restartScreenObject;
    [SerializeField] private SlingShotHandler slingshotHandler;
    [SerializeField] private Image nextLevelImage;
    [SerializeField] private GetSensorsInput sensorsInput;
    private float prevForce=0f;
    private int usedNumberOfShots;
    private IconHandler iconHandler;
    

    private List<Piggy> _piggies = new List<Piggy>();

    private void Awake() {
        if(instance==null){
            instance=this;
        }
        iconHandler = FindObjectOfType<IconHandler>();
        Piggy[] piggies = FindObjectsOfType<Piggy>();
        for (int i = 0; i < piggies.Length; i++)
        {
            _piggies.Add(piggies[i]);
        }
    }
    private void Update(){
        if (!slingshotHandler.enabled){
              if(prevForce < 10 && sensorsInput.force>= 10){
            //was pressed this frame
                NextLevel();


            }
        }
        prevForce = sensorsInput.force;
    }

    public void UseShot(){
        usedNumberOfShots++;
        iconHandler.UseShot(usedNumberOfShots);
        CheckForLastShot();
    }
    public bool HasEnoughShots(){
        if(usedNumberOfShots<MaxNumberofShots){
            return true;
        }
        else{
            return false;
        }
    }
    public void CheckForLastShot(){
        if(usedNumberOfShots==MaxNumberofShots){
            //wait
            StartCoroutine(CheckAfterWaitTime());
        }
    }
    private IEnumerator CheckAfterWaitTime(){
        yield return new WaitForSeconds(secondsToWaitBeforeDeathCheck);
        if(_piggies.Count == 0){
            //win
            WinGame();
        }
        else{
            //lose
            RestartGame();
        }

        //have all pigs been killed
    }
    public void RemovePiggy(Piggy piggy){
        _piggies.Remove(piggy);
        CheckForAllDeadPiggies();
    }
    private void CheckForAllDeadPiggies(){
        if(_piggies.Count == 0){
            //win
            WinGame();
        }
    }

    #region Win/Lose
    private void WinGame () {
        restartScreenObject.SetActive(true);
        slingshotHandler.enabled = false;
        //do we have any more levels to load?
        nextLevelImage.enabled = true;
     
    }
    public void RestartGame () {
        //maybe make a menu popup, but this also works
        DOTween.Clear(true);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void NextLevel(){
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int maxLevels = SceneManager.sceneCountInBuildSettings;
         if(currentSceneIndex+1<maxLevels){
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex+1);
        }
        else{
            SceneManager.LoadScene(0);
        }
        
    }
    #endregion
}
