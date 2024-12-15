::设置分支名字
SET ToolName=upm
::设置模块源路径
SET ToolAssetPath=Assets/ybwork.GPUSkinning

::此命令会创建一个ToolName的分支，并同步ToolAssetPath下的内容，并推送到origin仓库
git subtree push -P %ToolAssetPath% origin %ToolName%

pause
