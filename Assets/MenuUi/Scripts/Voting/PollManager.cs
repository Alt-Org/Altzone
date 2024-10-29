using System;
using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;

public static class PollManager
{
    private static List<PollObject> pollObjectList = new List<PollObject>();


    public static void CreatePollObject(PollType pollType, string id, int durationInHours, Sprite sprite, EsinePollType esinePollType, float value)
    {
        string name = "joku";

        long endTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + durationInHours * 3600;

        PollObject pollObject = new EsinePollObject(pollType, id, name, endTime, sprite, esinePollType, value);
        pollObjectList.Add(pollObject);

        PrintPollObjectList();
    }

    public static void CreatePollObject(PollType pollType, string id, int durationInHours, Sprite sprite, FurniturePollType furniturePollType, GameFurniture furniture, double weight, float value)
    {
        string name = "joku";

        long endTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + durationInHours * 3600;

        PollObject pollObject = new FurniturePollObject(pollType, id, name, endTime, sprite, furniturePollType, furniture, weight, value);
        pollObjectList.Add(pollObject);

        //PrintPollObjectList();
    }

    private static void PrintPollObjectList()
    {
        for (int i = 0; i < pollObjectList.Count; i++)
        {
            PollObject pollObject = pollObjectList[i];

            DateTime dateTimeEnd = DateTimeOffset.FromUnixTimeSeconds(pollObject.EndTime).DateTime;

            // Basic information common to all PollObjects
            string output = $"Poll Object {i + 1}: ID={pollObject.Id}, Name={pollObject.Name}, Time={dateTimeEnd}, Type={pollObject.PollType}";

            // Check the type of pollObject and append specific information
            if (pollObject is FurniturePollObject furniturePoll)
            {
                output += $", FurniturePollType={furniturePoll.FurniturePollType}, Weight={furniturePoll.Weight}, Value={furniturePoll.Value}";
            }
            else if (pollObject is EsinePollObject esinePoll)
            {
                output += $", EsinePollType={esinePoll.EsinePollType}, Value={esinePoll.Value}";
            }

            Debug.Log(output);
        }
    }

    public static List<PollObject> GetPollObjectList()
    {
        return pollObjectList;
    }
}
