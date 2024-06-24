using System.Collections.Generic;
using JetBrains.Annotations;
using PooledScrollList.Controller;
using PooledScrollList.Data;
using UnityEngine;
using UnityEngine.UI;

namespace PooledScrollList.Example
{
    public class PooledDataProviderExample : PooledDataProvider
    {
        public PooledScrollRectBase ScrollRectController;
        public InputField InputField;
        public int Count;

        private void Awake()
        {
            InputField.text = Count.ToString();
        }

        public override List<PooledData> GetData()
        {
            //Assigning Data here...
            var data = new List<PooledData>(PlayfabConnet.instance.gameLeaderboardData.rankLeaderBoards.Count);

            foreach (var item in PlayfabConnet.instance.gameLeaderboardData.rankLeaderBoards)
            {
                data.Add(item);
            }
            return data;
        }

        [UsedImplicitly]
        public void Apply()
        {
            if (InputField != null && !string.IsNullOrEmpty(InputField.text))
            {
                if (int.TryParse(InputField.text, out int result))
                {
                    Count = result;
                }
            }

            var data = GetData();

            if (ScrollRectController != null)
            {
                ScrollRectController.Initialize(data);
            }
        }
    }
}