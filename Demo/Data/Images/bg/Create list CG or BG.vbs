'Как пользоваться: поместить в папку с бг/цг и запустить. Сгенерится готовый Links.txt, из которого надо скопировать туда, куда надо

Set FSO = CreateObject("Scripting.FileSystemObject")
Set F = FSO.GetFile(Wscript.ScriptFullName)
path = FSO.GetParentFolderName(F)
Set objFS = CreateObject("Scripting.FileSystemObject")
Bank = ""
For Each objItem In objFS.GetFolder(path).Files
    If Right(objItem.Name, 4) = ".png" Then
        tmpAdd = left(objItem.Name,len(objItem.Name) - 4)
        Bank = Bank &  "$im " & tmpAdd & " " & Mid(path,InStr(path,"Images\")) & "\" & RemoveExtension(objItem.Name) & vbCrlf
    End If
Next
Dim fso2, f1
Set fso2 = CreateObject("Scripting.FileSystemObject")
Set f1 = fso2.CreateTextFile(path & "\" & "Links.txt", True)
f1.Write(Bank) 
f1.Close

Function RemoveExtension(txt)
RemoveExtension = Left(txt, InStrRev(txt, ".") - 1)
End Function