using System.Collections.Generic;
using System.Data;
using Mono.Data.Sqlite;
using UnityEngine;

public class QuizDB
{
    private string GetDBPath()
    {
        //return "URI=file:" + Application.streamingAssetsPath +"/Quiz.db";
        return "URI=file:" + Application.streamingAssetsPath + "/test.db";
    }

    // 문제 리스트 불러오기
    public List<string> LoadQuestions()
    {
        List<string> questions = new List<string>();

        using (IDbConnection conn = new SqliteConnection(GetDBPath()))
        {
            conn.Open();
            using (IDbCommand cmd = conn.CreateCommand())
            {
                //cmd.CommandText = "SELECT question FROM Quiz";
                cmd.CommandText = "SELECT content FROM Question";
                using (IDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string q = reader.GetString(0);
                        questions.Add(q);
                    }
                }
            }
        }

        return questions;
    }
}
