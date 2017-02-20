# SQLiteManager

Unity3d (ver5.x) SQLite DB manager (using SQLiter plugin)

- SQLiteMgr.cs script 의 배포 및 확장이 주 목적입니다.
- SQLite Qurey 모듈 함수를 정리하여 범용적으로 사용하기 위함 입니다.
- 예제 소스는 자유롭게 공유 및 사용이 가능하며 Commit 시 빌드 후 에러가 없어야 합니다.

# Prerequisites

- Unity3D 5.5.0

# Dependencies

SQLiteManager는 아래 패키지를 사용합니다.

- [SQLiter](https://www.assetstore.unity3d.com/kr/#!/content/20660)

# Script Explanation

- Path 관련 Public 함수
```
SetDBName()
bool SetDBFilePath(string Path, string DBName, bIsAppDataPath = true)
bool SetDBFilePath(string DBFilePath)
*두 개의 함수로 오버라이딩 됨.
```
- DB 관련 Public 함수
```
int DataTableclumnCnt(string TableName)
int InsertData(string TableName, string Data)
List<System.Object[]> GetTableAllData(string TableName)
```
- 함수의 자세한 내용은 SQLiteMgr.cs 의 주석을 참고하시길 바랍니다.

# Comment

- DB 관련하여 Query 문을 관리하는 범용 소스가 없어서 제작하였습니다.
- Unity5 이상 버전 SQLite 사용 시 사용하시고 필요한 부분은 추가하여 commit 부탁합니다.
- 많이들 추가해주세요!
