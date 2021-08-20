# gitup
git multi repo fetch &amp; pull

# 주의사항
프로그램 처음 실행 후 Setting화면을 통해 AccessToken과 Root Path를 설정 후 사용하세요.

*AccessToken 생성은 아래 주소를 확인해주세요.  
https://docs.microsoft.com/ko-kr/azure/devops/organizations/accounts/use-personal-access-tokens-to-authenticate?view=azure-devops&tabs=preview-page*

# 참고
* 검색기능은 RepoName과 Branch 이름을 검색합니다.

## 기능
1. Fetch
2. Pull (* 충돌시 예외처리 기능 없습니다. 프로그램 죽을지도 모릅니다.)
3. Branch CheckOut (Only Local)
4. Commit List
5. Changes List
7. Local과 Remote의 브런치 개수 비교
8. Fork.exe 실행
9. 현재 상태값 확인

# 단축키
1. Ctrl + F : 검색 인풋박스 포커스
2. Ctrl + Enter : 전체 Fetch
3. Ctrl + T : 선택된 Repo Fetch
4. Ctrl + G : 선택된 Repo의 Fork실행
5. F5 : Root Path Reload
