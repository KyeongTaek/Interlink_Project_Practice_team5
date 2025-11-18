using UnityEngine;
using System.Data;
using Mono.Data.Sqlite;

public class DbTest : MonoBehaviour
{
    IDbConnection getConnection(string dbname)
    {
        string connectionString = "URI=file:" + Application.streamingAssetsPath + dbname;
        IDbConnection dbConnection = new SqliteConnection(connectionString);
        dbConnection.Open();

        return dbConnection;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        string dbname = "/testdb.db";
        IDbConnection dbConnection = getConnection(dbname);

        string tablename = "Test";

        IDbCommand dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = "SELECT * FROM " + tablename;
        IDataReader dataReader = dbCommand.ExecuteReader();

        while (dataReader.Read())
        {
            int questionNumber = dataReader.GetInt32(0);
            int sceneNumber = dataReader.GetInt32(1);
            string questionData = dataReader.GetString(2);
            string questionAnswer = dataReader.GetString(3);
            Debug.Log("questionNumber: " + questionNumber + ", sceneNumber: " + sceneNumber + ", questionData: " + questionData + ", questionAnswer: " + questionAnswer);
        }
        dataReader.Close();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}