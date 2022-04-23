using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using Random = System.Random;
using UnityEngine.Events;
using UnityEngine.UI;

public class WheelSpin : NetworkBehaviour
{
    public Rigidbody wheelRb;
    public UnityEvent disable;
    public UnityEvent enable;
    public bool spunWheel;
    public float threshhold;
    public bool hitMaxVelocity = false;

    public int moneyBetting = 0;
    public int incrementAmount = 0;

    public GameObject incrementText;
    public GameObject betAmountText;

    public List<GameObject> pointMultipliers = new List<GameObject>();
    public List<TextMeshProUGUI> multiText = new List<TextMeshProUGUI>();
    public int numberOfTries;

    public GameObject currentSelection;

    public TextMeshProUGUI resultsText;
    
    [ServerCallback]
    void Start()
    {
        ShuffleRewards();
        StartCoroutine(DisableText());
        incrementIncrementAmount();
    }

    public void SpinTheWheel()
    {
        CmdSpinTheWheel();
    }

    [Command(ignoreAuthority = true)]
    private void CmdSpinTheWheel(NetworkConnectionToClient sender = null)
    {
        spunWheel = true;
        //This will remove the amount you bet
        sender.identity.GetComponent<Player>().AddGold(-moneyBetting);
        disable?.Invoke();
        wheelRb.AddTorque(new Vector3(UnityEngine.Random.Range(100000, 200000), 0, 0));
    }

    void GetRewards(float multiplier)
    {
        CmdSendRewards(multiplier);
    }


    [Command(ignoreAuthority = true)]
    void CmdSendRewards(float multiplier,NetworkConnectionToClient sender = null)
    {
        sender.identity.GetComponent<Player>().AddGold(Mathf.RoundToInt(moneyBetting*multiplier));
        DisplayResults(moneyBetting*multiplier);
    }


    private void Update()
    {
        if (spunWheel && Mathf.RoundToInt(wheelRb.angularVelocity.x) >= threshhold)
        {
            hitMaxVelocity = true;
        }

        if (System.Math.Round(wheelRb.angularVelocity.x, 2) == 0.00f && hitMaxVelocity)
        {
            wheelRb.angularVelocity = new Vector3(0, 0, 0);
            enable?.Invoke();
            hitMaxVelocity = false;
            spunWheel = false;
            GetRewards(currentSelection.GetComponent<PointMultiplyer>().multiplierAmount);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        currentSelection = other.gameObject;
    }

    [Server]
    public void ShuffleRewards()
    {
        //TODO Depending on how this goes in playtesting, this might get changed to a full weight system
        var fiveCount = 0;
      
        //Generate intial values
        foreach (GameObject multiplier in pointMultipliers)
        {
            var putAFive = 0;
            var script = multiplier.GetComponent<PointMultiplyer>();
            var randomNumber = 0;
            putAFive = UnityEngine.Random.Range(0, 100) + 1;
            if (putAFive < 2)
            {
                randomNumber = UnityEngine.Random.Range(-1, 5) + 1;
            }
            else
            {
                randomNumber = UnityEngine.Random.Range(-1, 4) + 1;
            }

            if (randomNumber.Equals(1))
                script.multiplierAmount = 1.5f;

            if (randomNumber.Equals(5))
                fiveCount++;

            script.multiplierAmount = randomNumber;
        }
        //if there is a 5, set at least 3 (Subject to change) values to 0x and the other 4 to 1.5-3x
        if (fiveCount > 0)
        {
            for (int x = 0; x < pointMultipliers.Count; x++)
            {
                pointMultipliers[UnityEngine.Random.Range(0, pointMultipliers.Count)]
                    .GetComponent<PointMultiplyer>().multiplierAmount = 0;
            }

            for (int x = 0; x < 3; x++)
            {
                var randomNumber = UnityEngine.Random.Range(1, pointMultipliers.Count);

                if (randomNumber == 1)
                {
                    pointMultipliers[UnityEngine.Random.Range(0, pointMultipliers.Count)]
                        .GetComponent<PointMultiplyer>().multiplierAmount = 1.5f;
                }

                pointMultipliers[UnityEngine.Random.Range(0, pointMultipliers.Count)]
                    .GetComponent<PointMultiplyer>().multiplierAmount = 0;
            }

            for (int x = 0; x < 1; x++)
            {
                var randomNumber = UnityEngine.Random.Range(0, pointMultipliers.Count);

                if (randomNumber < 5)
                {
                    x--;
                }
                else
                {
                    pointMultipliers[UnityEngine.Random.Range(0, pointMultipliers.Count)]
                        .GetComponent<PointMultiplyer>().multiplierAmount = 5;
                }
            }
        }


        //Update text
        for (int x = 0; x < multiText.Count; x++)
        {
            var script = pointMultipliers[x].GetComponent<PointMultiplyer>();
            multiText[x].SetText(script.multiplierAmount.ToString() + "x");
        }
    }


    #region IncrementValues

    public void incrementBettingMoney()
    {
        moneyBetting += incrementAmount;
    }

    public void decreaseBettingMoney()
    {
        moneyBetting -= incrementAmount;
       
    }

    public void incrementIncrementAmount()
    {
        //This might change to 50.. we will see play test wise
        incrementAmount += 25;
    }

    public void deincrementIncrementAmount()
    {
        incrementAmount -= 25;
        if (incrementAmount <= 0)
        {
            incrementAmount = 0;
        }
    }

    #endregion

    #region Display Results
    public void DisplayResults(float amount)
    {
        if (amount.Equals(0))
        {
           resultsText.SetText("Better luck next time....");
        }
        else
        {
            resultsText.SetText("You won: " + amount + " gold pieces");
        }

        StartCoroutine(DisableText());
    }

    IEnumerator DisableText()
    {
        yield return new WaitForSeconds(2);
        resultsText.SetText("");
    }

    #endregion
}