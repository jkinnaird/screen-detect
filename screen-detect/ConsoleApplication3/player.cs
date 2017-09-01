using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication3
{
    class player
    {
        private string playerName;
        private int contentType;
        private int xRes, yRes;
        private int resolution;

        public int[,,,,] values = new int[5,2,15,3,4];  //point, direction, pixel, color, pass
        public bool[,] isBlack = new bool[5, 4];        //point, pass

        public void setPlayerName(string input) { this.playerName = input; }
        public string getPlayerName(){ return this.playerName; }

        public void setContentType(int input) { this.contentType = input; }
        public int getContentType() { return this.contentType; }

        public void setxRes(int input) { this.xRes = input; }
        public int getxRes() { return this.xRes; }

        public void setyRes(int input) { this.yRes = input; }
        public int getyRes() { return this.yRes; }

        public void setResolution(int input) { this.resolution = input; }
        public int getResolution() { return this.resolution; }
    }
}
