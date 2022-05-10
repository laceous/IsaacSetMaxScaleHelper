# IsaacSetMaxScaleHelper

This was part of a POC for the [Set Max Scale](https://github.com/laceous/isaac-mod-set-max-scale) mod. Setting MaxScale while Isaac is running doesn't do anything by itself. You need to trigger an update by toggling fullscreen or resizing the window. You can toggle fullscreen from Lua code. This was an attempt to resize the window via the [SetWindowPos](https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowpos) function.

Ultimately, I wasn't very happy with this. I don't think it behaves any better than toggling fullscreen, and it opens up a security risk.

## Instructions

1. Run the IsaacSetMaxScaleHelper program
2. Run Isaac with `--luadebug`
3. Use the following Lua code instead of toggling fullscreen

```lua
local socketLoaded, socket = pcall(require, 'socket')

if socketLoaded then
  local tcp = socket.tcp()
  tcp:connect('localhost', 11568)
  tcp:send('IsaacSetMaxScaleHelper')
  tcp:close()
end
```