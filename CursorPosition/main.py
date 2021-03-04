import win32gui as win32

print("Ok")


while True:
    point = win32.GetCursorPos()
    print(point)
