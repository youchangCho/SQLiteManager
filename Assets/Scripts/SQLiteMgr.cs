using UnityEngine;
using System.Collections;
using System.Data;
using Mono.Data.SqliteClient;
using System.Collections.Generic;
using System;

/// <summary>
/// SQLite DB 와 통신 하기 위한 Class
/// SQLiter Asset(plugIn) 이 필요 합니다.
/// </summary>
public class SQLiteMgr : MonoBehaviour
{
    #region DB 통신을 위한 변수

    private IDbConnection m_DBConnection = null;
    private IDbCommand m_DBCommand = null;
    private IDataReader m_DBReader = null;

    private bool m_bIsConnect = false;

    #endregion DB 통신을 위한 변수

    #region DB 통신 기본 Query 문

    private const string ALL_SELECT = "SELECT * FROM ";
    private const string GET_TABLE_CNT = "SELECT count(*) FROM ";
    private const string INSERT_DATA = "INSERT INTO ";

    #endregion DB 통신 기본 Query 문

    #region DB 경로 관련 변수

    //DB 파일 전체 경로
    private string m_SqlDBFileFullUrl = "";

    //루트 부터 DB 파일 경로
    private string m_SqlDBFilePath = "";

    private string m_SqlDBName = "";

    #endregion DB 경로 관련 변수

    #region Path 관련 Public 함수

    /// <summary>
    /// DB파일 이름을 입력합니다. (확장자 까지 입력이 필요 합니다.)
    /// </summary>
    /// <param name="DBName">확장자를 포함한 DB 이름</param>
    public void SetDBName(string DBName)
    {
        if (DBName == "")
        {
            return;
        }

        m_SqlDBName = DBName;
        ChangeDBFile(m_SqlDBName);
    }

    /// <summary>
    /// DB 파일의 전체 경로를 설정 합니다.
    /// AppDataPath flag 설정시 AppData경로를 제외한 경로를 입력합니다.
    /// </summary>
    /// <param name="Path">DB File을 제외한 경로</param>
    /// <param name="DBName">DB file 이름</param>
    /// <param name="bIsAppDataPath">AppDataPath 경로 사용 유무(디폴트 true)</param>
    /// <returns>성공시 true, 그 외 false</returns>
    public bool SetDBFilePath(string Path, string DBName, bool bIsAppDataPath = true)
    {
        if (Path == "" || DBName == "")
        {
            return false;
        }

        m_SqlDBFilePath = Path;

        if (bIsAppDataPath == true)
        {
            if (!CheckPathStartSeparator(ref m_SqlDBFilePath))
            {
                m_SqlDBFilePath = "";
                return false;
            }

            if (!CheckPathEndSeparator(ref m_SqlDBFilePath))
            {
                m_SqlDBFilePath = "";
                return false;
            }

            m_SqlDBFilePath = Application.dataPath + m_SqlDBFilePath;
        }

        m_SqlDBFileFullUrl = "URI=file:" + m_SqlDBFilePath + DBName;

        return true;
    }

    /// <summary>
    /// DB 파일 이름을 제외한 경로를 설정 합니다.
    /// </summary>
    /// <param name="DBFilePath">DB 파일을 제외한 경로</param>
    public void SetDBFilePath(string DBFilePath)
    {
        if (DBFilePath == "")
        {
            return;
        }

        m_SqlDBFilePath = DBFilePath;

        if (!CheckPathEndSeparator(ref m_SqlDBFileFullUrl))
        {
            m_SqlDBFileFullUrl = "";
        }
    }

    #endregion Path 관련 Public 함수

    #region Path 관련 private 함수

    /// <summary>
    /// DB 파일을 변경 합니다 .
    /// </summary>
    /// <param name="DBFileName">변경하고자 하는 DB 파일 명(확장자 포함)</param>
    private void ChangeDBFile(string DBFileName)
    {
        if (DBFileName == "" || m_SqlDBFileFullUrl == "")
        {
            return;
        }

        int End_Separator = m_SqlDBFileFullUrl.LastIndexOf("/") + 1;

        m_SqlDBFileFullUrl = m_SqlDBFileFullUrl.Substring(0, End_Separator) + DBFileName;
    }

    /// <summary>
    /// 경로 마지막 문자열에 구분자'/' 가 있는지 체크 하고 없으면 입력 합니다.
    /// </summary>
    /// <param name="path">체크할 경로</param>
    /// <returns>성공시 true, 그 외 false</returns>
    private bool CheckPathEndSeparator(ref string path)
    {
        if (path == "")
        {
            return false;
        }

        string endchar = path.Substring(path.Length - 1);
        if (endchar != "/")
        {
            path += "/";
        }
        return true;
    }

    /// <summary>
    /// 경로 시작 문자열에 구분자'/' 가 있는지 체크 하고 없으면 입력 합니다.
    /// </summary>
    /// <param name="path">체크할 경로</param>
    /// <returns>성공시 true, 그 외 false</returns>
    private bool CheckPathStartSeparator(ref string path)
    {
        if (path == "")
        {
            return false;
        }

        string endchar = path.Substring(path.Length - 1);
        if (endchar != "/")
        {
            path += "/";
        }
        return true;
    }

    #endregion Path 관련 private 함수

    #region DB 접속 관련 private 함수

    /// <summary>
    /// DB 접속과 초기화를 하기 위한 함수 입니다.
    /// Connect 된 후 연결을 유지 합니다.
    /// 반드시 SQLiteDisConnect() 를 하십시오.
    /// </summary>
    /// <returns>접속이 되면 1, 접속이 되지 않으면 -1, DB경로가 없으면 -2</returns>
    private int Connect()
    {
        if (m_SqlDBFileFullUrl == "")
        {
            return -2;
        }

        #region SQLite 기본 속성 정의

        Debug.Log("SQLiter - Opening SQLite Connection at " + m_SqlDBFileFullUrl);
        m_DBConnection = new SqliteConnection(m_SqlDBFileFullUrl);
        m_DBConnection.Open();

        if (m_DBConnection.State != ConnectionState.Open)
        {
            m_DBConnection = null;
            m_bIsConnect = false;
            return -1;
        }

        try
        {
            m_DBCommand = m_DBConnection.CreateCommand();

            // WAL = write ahead logging, very huge speed increase
            m_DBCommand.CommandText = "PRAGMA journal_mode = WAL;";
            m_DBCommand.ExecuteNonQuery();

            // journal mode = look it up on google, I don't remember
            m_DBCommand.CommandText = "PRAGMA journal_mode";
            m_DBReader = m_DBCommand.ExecuteReader();
#if UNITY_EDITOR
            if (m_DBReader.Read())
                Debug.Log("SQLiter - WAL value is: " + m_DBReader.GetString(0));
            m_DBReader.Close();
#endif
            // more speed increases
            m_DBCommand.CommandText = "PRAGMA synchronous = OFF";
            m_DBCommand.ExecuteNonQuery();

            // and some more
            m_DBCommand.CommandText = "PRAGMA synchronous";
            m_DBReader = m_DBCommand.ExecuteReader();
#if UNITY_EDITOR
            if (m_DBReader.Read())
                Debug.Log("SQLiter - synchronous value is: " + m_DBReader.GetInt32(0));
            m_DBReader.Close();
#endif
        }
        catch (Exception e)
        {
            Debug.Log(e);
            return -1;
        }

        #endregion SQLite 기본 속성 정의

        return 1;
    }

    /// <summary>
    /// SQLite를 닫습니다.
    /// </summary>
    private void DisConnect()
    {
        if (m_DBConnection != null)
        {
            m_DBConnection.Close();
            m_DBConnection = null;
        }
        if (m_DBCommand != null)
        {
            m_DBCommand.Dispose();
            m_DBCommand = null;
        }
        if (m_DBReader != null)
        {
            m_DBReader.Close();
            m_DBReader = null;
        }
    }

    /// <summary>
    /// 쿼리 실행 및 단일 데이터 리턴
    /// </summary>
    /// <param name="commandText">쿼리문</param>
    /// <returns>단일 데이터(배열처리 하지 않은 string)</returns>
    private string ExecuteQuery_and_data(string commandText)
    {
        if (commandText == "")
        {
            return "";
        }
        //일단 수시로 열고 닫는거로 테스트 by imagej
        //m_DBConnection.Open();
        if (Connect() != 1)
        {
            return "";
        }

        string value = "0";

        try
        {
            m_DBCommand.CommandText = commandText;
            IDataReader reader = m_DBCommand.ExecuteReader();
            int cnt = reader.FieldCount;

            while (reader.Read())
            {
                value = reader.GetString(0);
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
            value = "";
        }

        DisConnect();

        return value;
    }

    /// <summary>
    /// Query 단일 실행문 함수, 리턴이 없는 실행을 하기 위한 모듈 입니다.
    /// </summary>
    /// <param name="commandText">Query 명령어</param>
    /// <returns>성공시 1, Query 문 에러시 -1, DB 접속 실패시 -2</returns>
    private int ExecuteNonQuery(string commandText)
    {
        if (commandText == "")
        {
            return -1;
        }

        if (Connect() != 1)
        {
            return -2;
        }

        try
        {
            m_DBCommand.CommandText = commandText;
            m_DBCommand.ExecuteNonQuery();
        }
        catch (Exception e)
        {
            Debug.Log(e);
            DisConnect();
            return -1;
        }

        DisConnect();

        return 1;
    }

    #endregion DB 접속 관련 private 함수

    #region DB 관련 publi 함수

    /// <summary>
    /// TableName의 Table 컬럼 개수를 리턴 합니다.
    /// </summary>
    /// <param name="TableName">테이블 명</param>
    /// <returns>성공시 해당 table 컬럼 개수 리턴, TableName 이 없으면 -1, DB 연결이 않되어 있으면 -2</returns>
    public int DataTablecolumnCnt(string TableName)
    {
        //if (m_DBConnection == null)
        //{
        //    return -2;
        //}
        if (TableName == "")
        {
            return -1;
        }

        string sqlquery = GET_TABLE_CNT + TableName + ";";
        int cnt = 0;

        if (int.TryParse(ExecuteQuery_and_data(sqlquery), out cnt) == false)
        {
            Debug.Log("DataTablecolumnCnt:: DB count is not number");
            return -2;
        }

        return cnt;
    }

    /// <summary>
    /// SQLite 에 데이터를 삽입 합니다.
    /// </summary>
    /// <param name="TableName">삽입하고자 하는 Table 명</param>
    /// <param name="Data">데이터(구분자(,)와 괄호를 포함한 데이터 셋</param>
    /// <returns>성공시 1, 파라미터 에러시 -1, DB 접속 실패시 -2</returns>
    public int InsertData(string TableName, string Data)
    {
        if (TableName == "" || Data == "")
        {
            return -1;
        }

        string sqlquery = INSERT_DATA + TableName + Data;

        return ExecuteNonQuery(sqlquery);
    }

    /// <summary>
    /// SQLite 의 특정 테이블의 데이터를 모두 받아 온다.
    /// </summary>
    /// <param name="TableName">Table 이름</param>
    /// <returns>성공시 System.Object 2차원 배열, 실패시 null</returns>
    public List<System.Object[]> GetTableAllData(string TableName)
    {
        if (TableName == "")
        {
            return null;
        }

        int col_cnt = DataTablecolumnCnt(TableName);
        if (col_cnt <= 0)
        {
            return null;
        }
        string sqlquery = ALL_SELECT + TableName;

        if (Connect() != 1)
        {
            return null;
        }

        m_DBCommand.CommandText = sqlquery;
        m_DBReader = m_DBCommand.ExecuteReader();

        int FieldCnt = m_DBReader.FieldCount;
        if (FieldCnt <= 0)
        {
            DisConnect();

            return null;
        }

        List<System.Object[]> returnObjData = new List<System.Object[]>();
        int i = 0;
        while (m_DBReader.Read())
        {
            System.Object[] obj = new System.Object[FieldCnt];
            returnObjData.Add(obj);
            m_DBReader.GetValues(returnObjData[i]);
            i++;
        }

        DisConnect();
        return returnObjData;
    }

    #endregion DB 관련 publi 함수
}