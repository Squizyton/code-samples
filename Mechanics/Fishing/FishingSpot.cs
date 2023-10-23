using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SysRandom = System.Random;

namespace Fishing
{
    public class FishingSpot : MonoBehaviour
    {
        [SerializeField] private FishingManager _fishingManager;


        //List of available fish for this pond
        [SerializeField] private List<FishData> listoFish;

        private void Start()
        {
        
        }

        public void StartFishing()
        {

            var selectedFish = ReturnAFish(listoFish);
            Debug.Log(selectedFish);
            
            //Send the fish to the Fishing Manager
            _fishingManager.StartBiteTimer(selectedFish);
        }

        FishData ReturnAFish(List<FishData> list)
        {
            var timeOfDay = DayNightCycle.instance.GetTimeOfDay();
            Debug.Log(timeOfDay);
            return Shuffle(list).FirstOrDefault(i => i.beginningTime <= timeOfDay && i.endTime >= timeOfDay);
        }

        private static SysRandom rng = new SysRandom();

        public static IList<T> Shuffle<T>(IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
            
            return list;
        }
    }
}