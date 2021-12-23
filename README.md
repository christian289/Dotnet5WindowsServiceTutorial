# Dotnet5WindowsServiceTutorial
.NET 5를 이용하여 Windows Service를 만들어 봅시다.

[Visual Studio 2019의 작업자 서비스 템플릿](https://docs.microsoft.com/ko-kr/dotnet/core/extensions/windows-service) 을 사용하여 .NET 5로 Windows Service를 만드는 튜토리얼 입니다.

소스코드는 위의 링크대로 작성하시면 되며, 수정할 부분은 프로젝트 파일입니다.

시작하기 전, 반드시 __Visual Studio를 관리자 권한으로 실행__ 해주세요. 기본적으로 Windows Service는 관리자 권한이 있을 때만 등록하고, 삭제하고 변경할 수 있습니다.

```xml
<Target Name="PostBuild" AfterTargets="PostBuildEvent">
    <Exec Command="sc.exe create $(ProjectName) binpath=&quot;$(TargetDir)$(ProjectName).exe&quot;&#xD;&#xA;sc.exe start $(ProjectName)" />
  </Target>

  <Target Name="PreBuild" BeforeTargets="PreBuildEvent">
    <Exec Command="sc.exe query state=all | findstr $(ProjectName)&#xD;&#xA;if %25errorlevel%25 equ 0 (&#xD;&#xA;sc.exe stop $(ProjectName)&#xD;&#xA;sc.exe delete $(ProjectName)&#xD;&#xA;) else (&#xD;&#xA;echo exit&#xD;&#xA;exit 0&#xD;&#xA;)" />
</Target>
```

위 부분을 프로젝트 파일에 추가해주시면 되는데, 추가하고 나면

```xml
<Project Sdk="Microsoft.NET.Sdk.Worker">

  <PropertyGroup>
    <TargetFramework>net5.0</TargetFramework>
    <PublishSingleFile>true</PublishSingleFile>
    <OutputType>exe</OutputType>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="5.0.2" />
    <PackageReference Include="Microsoft.Extensions.Hosting" Version="5.0.0" />
    <PackageReference Include="Microsoft.Extensions.Hosting.WindowsServices" Version="5.0.1" />
    <PackageReference Include="Microsoft.Extensions.Http" Version="5.0.0" />
  </ItemGroup>

  <Target Name="PostBuild" AfterTargets="PostBuildEvent">
    <Exec Command="sc.exe create $(ProjectName) binpath=&quot;$(TargetDir)$(ProjectName).exe&quot;&#xD;&#xA;sc.exe start $(ProjectName)" />
  </Target>

  <Target Name="PreBuild" BeforeTargets="PreBuildEvent">
    <Exec Command="sc.exe query state=all | findstr $(ProjectName)&#xD;&#xA;if %25errorlevel%25 equ 0 (&#xD;&#xA;sc.exe stop $(ProjectName)&#xD;&#xA;sc.exe delete $(ProjectName)&#xD;&#xA;) else (&#xD;&#xA;echo exit&#xD;&#xA;exit 0&#xD;&#xA;)" />
  </Target>
</Project>
```

위와 같이 됩니다.

물론 이렇게 하지 않고 프로젝트 속성 -> 빌드 이벤트에서 __빌드 이벤트 명령줄 대화 상자__ 와 __빌드 후 이벤트 명령줄__ 에 명령어를 작성하셔도 됩니다.
이 명령어는 기본적으로 Windows에 기본으로 설치되어 있는 __명령 프롬프트__ 에서 실행시킬 수 있는 [CMD](https://docs.microsoft.com/ko-kr/windows-server/administration/windows-commands/cmd) 명령어입니다.

명령어를 해석해보겠습니다.
__CMD 명령어는 Enter치면서 한 줄 한 줄 실행시키면 됩니다.__

> sc.exe query state=all | findstr $(ProjectName)

sc.exe 는 Windows Service를 관리하는 도구입니다. query 라는 부속 명령어와 state 파라미터를 통해 모든 state의 Service 목록을 가져오고 있는데, 이것을 | 로 findstr에 넘겨주어 $(ProjectName)이 포함되는 서비스를 검색합니다.
$(ProjectName)은 [MSBuild 에서 미리 예약된 Macro](https://docs.microsoft.com/ko-kr/visualstudio/ide/reference/pre-build-event-post-build-event-command-line-dialog-box?view=vs-2022) 입니다. Visual Studio는 사실 껍데기이고, Visual Studio가 MSBuild에 이것저것 넘겨주어 컴파일 & 빌드를 수행하는 것입니다. 아마도 docker 이미지를 만들기 위해 pipeline을 만들어보신 분들은 아실 것 같습니다.
위 명령을 실행하고나면 ERRORLEVEL 이라는 환경변수에 CMD 명령어를 수행한 Return value가 저장됩니다. CMD에서 위 명령어를 실행 후, echo %errorlevel% 또는 echo %ERRORLEVEL% 을 통해 Return 값을 조회해볼 수 있습니다.
아마도 __찾아낸 값이 있다면 0__ 을, __찾아낸 항목이 없다면 1을 반환__ 할 것입니다.
이 프로젝트를 Clone 한 후, 처음으로 Build를 수행하면 당연히 아직 Windows Service가 없으므로, 위 명령어의 실행결과는 1이 나올 것입니다.
그래서 다음,

> if %errorlevel% equ 0 (

이 수행결과인 errorlevel의 값을 가져와서 0 과 동등한지 비교한다는 것입니다. equ는 equal 의 약어입니다. 만약 여기서 아래의 조건으로 하면 원하는 대로 동작을 안하게 됩니다.

```text
if errorlevel 0 -> 0 이상
if errorlevel=0 -> 0 이상
if errorlevel = 0 -> 0 이상
if errorlevel==0 -> 0 이상
if errorlevel == 0 -> 0 이상
if errorlevel equ 0 -> 틀린 문법
if %errorlevel% 0 -> 틀린 문법
if %errorlevel% = 0 -> 틀린 문법
```

[이 댓글](https://stackoverflow.com/a/25019949/11536747)을 참고해도 좋지만 직접 CMD에 명령어를 입력해보면서 체크할 수도 있습니다.

아래 같은 용법만 정확하게 비교 가능합니다.

```text
if %errorlevel% == 0
if %errorlevel% equ 0
```

> sc.exe stop $(ProjectName)

sc.exe를 이용해 내 Visual Studio 프로젝트 이름과 같은 Windows Service를 중지시킵니다.

> sc.exe delete $(ProjectName)

sc.exe를 이용해 내 Visual Studio 프로젝트 이름과 같은 Windows Services를 삭제합니다.

> ) else (

만약 if 조건과 틀렸다면 else문을 타게됩니다.

> exit 0

만약 위 명령어를 빼놓고 빌드한다면 MSBuild가 빌드하지 못합니다. 처음 빌드를 시도할 경우엔 Windows Service가 없는데 이 때는 결과값이 1이나오고 0이 아니기 때문에 에러라고 판단하고 MSBuild가 빌드를 중단합니다.
또한 exit만 입력해도 Visual Studio가 오류로 반환합니다. 어떤 숫자든 꼭 exit 다음에 와야합니다.
__왜 숫자를 꼭 기입해야하는지는 잘 모르겠습니다. 0이외에 다른 숫자를 입력해도 정상 빌드됩니다. 알고 계신 분이 계시다면 피드백 부탁드립니다.__

여기까지하면 만약에 Windows Service가 이미 Visual Studio 프로젝트명과 같은 이름으로 실행되고 있는게 있었다면, 제거될 것입니다.
이것을 제거하지 않으면 Windows Service가 계속 실행중이며, MSBuild는 Build를 완료하고 새로 Build한 결과물을 해당 배포 경로에 배포할텐데 __기존 파일들이 Windows Service에서 사용 중__ 이라서 충돌나게 됩니다.
그래서 위와 같은 Windows Service를 검사하고 삭제하는 __빌드 전 이벤트__ 가 필요한 것입니다.

다음은 빌드가 완료된 후 입니다.

> sc.exe create $(ProjectName) binpath="$(TargetDir)$(ProjectName).exe"

sc.exe를 사용해서 Windows Service를 만들고 등록하는 명령어입니다. binpath를 이용해서 Windows Service에 등록될 어셈블리 디렉토리를 지정합니다. $(TargetDir) 역시 MSBuild의 빌드 메크로입니다.

> sc.exe start $(ProjectName)

sc.exe를 사용해서 Windows Service를 시작합니다.

여기까지하면 Windows Service가 실행되고, 추가적으로 Windows Service의 설정이 필요하다면 ```sc.exe config``` 를 통해 빌드 후 이벤트에 지정하실 수 있습니다.

[문서](https://docs.microsoft.com/ko-kr/dotnet/core/extensions/windows-service)에도 나와있지만 정상 동작하고 있는지는 __이벤트 뷰어__ 를 통해 확인하시면 됩니다.
