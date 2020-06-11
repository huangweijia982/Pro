---@class CycleScrollRect
CycleScrollRect = class("CycleScrollRect")
---将 item 作为content的子物体，设置好位置和大小即可
--构造函数
function CycleScrollRect:ctor(scrollRect, maxNum, onUpdataItem,onEnd)
    ---@type UnityEngine.UI.ScrollRect
    self.sc = scrollRect
    self.gameObject = self.sc.gameObject
    self.transform = self.gameObject.transform
    self.rt=self.gameObject:GetComponent("RectTransform")
    if self.sc.content.childCount==0 then
        print("content下的item不能为空")
        return
    end
    ---@type UnityEngine.RectTransform[]
    self.itemRTLs = {}
    ---@type UnityEngine.RectTransform
    self.view = self.sc.viewport
    ---@type UnityEngine.GameObject
    self.item = self.sc.content:GetChild(0).gameObject
    self.item:SetActive(false)
    ---@type UnityEngine.RectTransform
    self.itemRT = self.item:GetComponent("RectTransform")
    self.itemRT.anchorMax=Vector2.New(0.5,1)
    self.itemRT.anchorMin=Vector2.New(0.5,1)
    self.maxNum = maxNum
    self.sc.content.pivot=Vector2.New(0,1)
    self:initColAndLine()
    self.sc.content.sizeDelta = Vector2.New(0, self.itemRT.rect.height * math.ceil(maxNum / self.numberOfColumns))
    self.onUpdataItem = onUpdataItem
    self.onEnd=onEnd


    for i = 1, self.lineOfPage*self.numberOfColumns do
        self:creatItem()
    end

    coroutine.start(function()
        coroutine.wait(1)
        self.sc.onValueChanged:AddListener(function()
            self:updataShow()
        end)
    end)
    self.gameObject:AddComponent(typeof(AWLuaBehaviour)):RegisterDestroy(function()
        self.sc.onValueChanged:RemoveAllListeners()
        self.sc = nil
        self.onUpdataItem=nil
        self.onEnd=nil
        self.itemRTLs = nil
        self = nil
    end)
end

--创建item
--isforward 前一页
function CycleScrollRect:creatItem(isforward)
    isforward = isforward or false
    ---@type UnityEngine.RectTransform
    local rt, item
    if #self.itemRTLs < self.numberOfColumns * self.lineOfPage then
        item = UnityEngine.GameObject.Instantiate(self.item, self.sc.content)
        item:SetActive(true)
        local curIndex=#self.itemRTLs+1
        item.name = curIndex
        rt=item:GetComponent("RectTransform")
        table.insert(self.itemRTLs, rt)
    else
        if isforward then
            rt = table.remove(self.itemRTLs, #self.itemRTLs)
            rt.gameObject.name = rt.gameObject.name - self.lineOfPage*self.numberOfColumns
            table.insert(self.itemRTLs, 1, rt)
        else
            rt = table.remove(self.itemRTLs, 1)
            rt.gameObject.name = rt.gameObject.name + self.lineOfPage*self.numberOfColumns
            table.insert(self.itemRTLs, rt)
        end
    end
    rt.anchoredPosition = self:getPs(tonumber(rt.gameObject.name))
    if self.onUpdataItem ~= nil then
        self.onUpdataItem(rt.gameObject, tonumber(rt.gameObject.name))
    end
end
--更新显示
function CycleScrollRect:updataShow()
    for i = 1, #self.itemRTLs do
        local ps = self.itemRTLs[i]:InverseTransformPoint(self.rt.anchoredPosition)
        --print(self.itemRTLs[i].anchoredPosition)
        if -ps.y-self.itemRT.rect.height > self.view.localPosition.y then
            --print("向上或者左拉")
            if self.itemRTLs[#self.itemRTLs].gameObject.name == tostring(self.maxNum) then
                --print("到底了")
                return
            end
            self:creatItem(false)
            break
        end
        if -ps.y+self.itemRT.rect.height < self.view.localPosition.y-self.view.rect.height then
            --print("向下或者右拉")
            if self.itemRTLs[1].gameObject.name=="1" then
                --print("到底了")
                if self.onEnd~=nil then
                    self.onEnd()
                end
                return
            end
            self:creatItem(true)
            break
        end
    end
end

--初始化行列
function CycleScrollRect:initColAndLine()
    self.numberOfColumns= math.floor(self.sc.viewport.rect.width/self.itemRT.rect.width)
    self.numberOfLines=math.ceil(self.maxNum/self.numberOfColumns)
    self.lineOfPage=math.ceil(self.sc.viewport.rect.height/self.itemRT.rect.height)+2--一页的行数，多二个作为缓冲，也就是实际克隆出来的item个数
end

--计算位置
function CycleScrollRect:getPs(index)
    local value=index%self.numberOfColumns
    local cum_x = value==0 and self.numberOfColumns or value
    local cum_y = math.ceil(index/self.numberOfColumns)
    return Vector2.New(self.itemRT.anchoredPosition.x+self.itemRT.rect.width * (cum_x-1), self.itemRT.anchoredPosition.y-self.itemRT.rect.height * (cum_y-1))
end
