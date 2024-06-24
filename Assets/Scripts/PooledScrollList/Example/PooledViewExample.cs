using System;
using PooledScrollList.Data;
using PooledScrollList.View;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Gilzoide.GradientRect;

namespace PooledScrollList.Example
{
    [Serializable]
    public class PooledDataExample : PooledData
    {
        public Color Color;
        public int Number;
        public int Rank;
        public string name;

    }

    public class PooledViewExample : PooledView
    {
        [SerializeField] private GameConfig gameConfig;
        [SerializeField] private TextMeshProUGUI rank;
        [SerializeField] private TextMeshProUGUI playerName;
        [SerializeField] private TextMeshProUGUI xp;
        [SerializeField] private TextMeshProUGUI gamePlayed;
        [SerializeField] private TextMeshProUGUI winRate;
        [SerializeField] private TextMeshProUGUI vol;
        [SerializeField] private GradientImage gradientImage;

        public override void SetData(PooledData data)
        {
            base.SetData(data);

            RankLeaderBoard leaderboardData = (RankLeaderBoard) data;
            xp.text = leaderboardData.XP.ToString() + " XP";
            rank.text = leaderboardData.rank.ToString();
            playerName.text = leaderboardData.name;
            vol.text = leaderboardData.gameVol.ToString();
            gamePlayed.text = leaderboardData.gamePlayed.ToString();
            winRate.text = leaderboardData.winRate.ToString();

            if (leaderboardData.rank < 4)
            {
                gradientImage.Gradient = GetGradient(leaderboardData.rank);
                rank.color = GetGradient(leaderboardData.rank).Evaluate(100);
            }
            gradientImage.gameObject.SetActive(leaderboardData.rank < 4);            

        }
        public void SetMyGradient()
        {

        }
        private Gradient GetGradient(int rank)
        {
            if (rank == 1)
                return gameConfig.gradients[0];
            if (rank == 2)
                return gameConfig.gradients[1];
            if (rank == 3)
                return gameConfig.gradients[2];
            return gameConfig.gradients[0];
        }
    }
}