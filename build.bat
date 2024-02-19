::设置模块名字
SET ToolName=ybwork.GPUSkinning
::设置模块版本
SET ToolVersion=upm
::设置模块源路径
SET ToolAssetPath=Assets/ybwork/GPUSkinning

git branch -D %ToolName%
git remote rm %ToolName%
::此命令会创建一个ToolName的分支，并同步ToolAssetPath下的内容
git subtree split -P %ToolAssetPath% --branch %ToolName%
:: 在ToolName分支设置标签ToolVersion节点
git tag -d %ToolVersion%
git tag %ToolVersion% %ToolName%

:: 推送到远端
git push origin -f %ToolName% %ToolVersion%
git push origin %ToolName%
pause
