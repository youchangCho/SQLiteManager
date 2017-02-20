using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DBClient : MonoBehaviour
{
    #region Test Table 구조체

    public struct ST_TEST1
    {
        public int n_Data1 { get; set; }

        public string str_data2 { get; set; }

        public string str_data3 { get; set; }

        public ST_TEST1(System.Object[] obj, int cnt)
        {
            if (cnt != 3)
            {
                n_Data1 = 0;
                str_data2 = "";
                str_data3 = "";
            }
            else
            {
                int integer = 0;
                if (int.TryParse(obj[0].ToString(), out integer))
                {
                    n_Data1 = integer;
                }
                else
                {
                    n_Data1 = 0;
                }
                str_data2 = obj[1].ToString();
                str_data3 = obj[2].ToString();
            }
        }
    }

    #endregion Test Table 구조체

    #region DB 경로 관련 변수

    //DB 파일 경로
    public string m_SqlDBFilePath = "/DBdata/";

    public string m_SqlDBName = "TestDB.db";

    //DB Table Name
    public string m_TableName = "Test";

    #endregion DB 경로 관련 변수

    #region Test 설정 변수

    private SQLiteMgr m_SQLiteMgr = new SQLiteMgr();

    //true : Test, false : Test1
    private bool m_bTable = true;

    public Text m_TextDBState;

    public Text m_TextData;

    #endregion Test 설정 변수

    #region DB TEST 함수

    public void GetDBData()
    {
        SetSQLiteDBPath();
        List<System.Object[]> AllData;
        AllData = m_SQLiteMgr.GetTableAllData(m_TableName);
        string state = "Tabel Data is error";
        if (AllData.Count == 0)
        {
            Debug.Log(state);
            m_TextDBState.text = state;
            return;
        }
        else
        {
            state = "Connect " + m_TableName + " DB";
            Debug.Log(state);
            m_TextDBState.text = state;
        }

        foreach (System.Object[] obj in AllData)
        {
            ST_TEST1 st_data = new ST_TEST1(obj, obj.Length);
            string DBData = st_data.n_Data1.ToString() + "," + st_data.str_data2 + "," + st_data.str_data3;
            Debug.Log(DBData);
            m_TextData.text = DBData;
        }
    }

    public void SetSQLiteDBPath()
    {
        //시스템 데이터 경로를 사용
        m_SQLiteMgr.SetDBFilePath(m_SqlDBFilePath, m_SqlDBName, true);
    }

    public void ChangeTableName()
    {
        m_bTable = !m_bTable;
        string State = "Change DB ";
        if (m_bTable == true)
        {
            m_TableName = "Test";
        }
        else
        {
            m_TableName = "Test2";
        }
        State += m_TableName;
        m_TextDBState.text = State;

        m_SQLiteMgr.SetDBName(m_TableName);
        GetDBData();
    }

    #endregion DB TEST 함수
}