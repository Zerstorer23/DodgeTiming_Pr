using Lex;
using System.Collections.Generic;

public class GameStatus
{
    public LexPlayer lastSurvivor;
    public LexPlayer lastDied;
    public int total;
    public int alive;
    public int alive_ourTeam;
    public int alive_otherTeam;
    public int dead;
    public bool onlyBotsRemain = true;
    public GameStatus(SortedDictionary<string, Unit_Player> unitDict, LexPlayer lastDied)
    {
        Team myTeam = LexNetwork.LocalPlayer.GetProperty(Property.Team, Team.HOME);
        this.lastDied = lastDied;
        foreach (Unit_Player p in unitDict.Values)
        {
            total++;
            if (p != null && p.gameObject.activeInHierarchy)
            {
                lastSurvivor = p.controller.Owner;
                alive++;
                if (lastSurvivor.IsHuman) {
                    onlyBotsRemain = false;
                }
                if (GameSession.gameModeInfo.isTeamGame)
                {
                    if (p.myTeam != myTeam)
                    {
                        alive_otherTeam++;
                    }
                    else
                    {
                        alive_ourTeam++;
                    }
                }
            }
            else
            {
                dead++;
            }
        }
        if (lastSurvivor == null) {
            lastSurvivor = lastDied;
        }
    }


      
    

    public override string ToString()
    {
        string o = "<color=#00c8c8>===============GameStat========================</color>\n";
            o+="Game mode : " + GameSession.gameModeInfo.gameMode + "\n";
        o += "Total Players:" + total + "\n";
        o += "Total Alive " + alive + "\n";
        o += "\t \t my team:" + LexNetwork.LocalPlayer.GetProperty(Property.Team, Team.HOME) + " ? " + alive_ourTeam + "\n";
        o += "\t \t other team:" + LexNetwork.LocalPlayer.GetProperty(Property.Team, Team.HOME) + " ? " + alive_otherTeam + "\n";
        o += "Total dead:" + dead + "\n";
        o += "Last survivor:" + lastSurvivor + "\n";
        o += "<color=#00c8c8>===========================================</color>\n";
        return o;

    }
}