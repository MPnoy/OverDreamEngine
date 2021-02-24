'Как пользоваться: поместить в папку с музлом и запустить. Сгенерится готовый Links.txt, из которого надо скопировать туда, куда надо

Set FSO = CreateObject("Scripting.FileSystemObject")
Set F = FSO.GetFile(Wscript.ScriptFullName)
path = FSO.GetParentFolderName(F)
Set objFS = CreateObject("Scripting.FileSystemObject")
Bank = ""
For Each objItem In objFS.GetFolder(path).Files
    If Right(objItem.Name, 4) = ".mp3" Then
        tmpAdd = left(objItem.Name,len(objItem.Name) - 4)
        tmpAdd = Replace(tmpAdd,"MPnoy", "")
        tmpAdd = Replace(tmpAdd,"SoulCold", "")
        tmpAdd = Replace(tmpAdd,"Samael_JS", "")
        tmpAdd = Replace(tmpAdd,"SBSL", "")
        tmpAdd = Replace(tmpAdd,"Dusty_Joe", "")
        tmpAdd = Replace(tmpAdd,"Dany_Gankoff", "")
        tmpAdd = Replace(tmpAdd,"C_SABS", "")
        tmpAdd = Replace(tmpAdd,"19b", "")
        tmpAdd = Replace(tmpAdd,"Alexsei_Jidkov", "")
        tmpAdd = Replace(tmpAdd,"Kukuruzkin", "")
        Do While Left(tmpAdd, 1) = "_" or Left(tmpAdd, 1) = "&" 
            tmpAdd = Mid(tmpAdd, 2)
        Loop
        tmpAdd = UCase(Left(tmpAdd,1)) & Mid(tmpAdd, 2)
        Bank = Bank &  "$au " & tmpAdd & " " & Mid(path,InStr(path,"Audio\")) & "\" & objItem.Name & vbCrlf
    End If
Next
Dim fso2, f1
Set fso2 = CreateObject("Scripting.FileSystemObject")
Set f1 = fso2.CreateTextFile(path & "\" & "Links.txt", True)
f1.Write(Bank) 
f1.Close