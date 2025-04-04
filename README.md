# Moderation
使用 C# ML.NET 训练的审核模型. 支持接入程序/API调用

## 判断内容

### 1. **sexual（性内容）**

   - **定义**：表示内容是否涉及性相关的话题、描述或暗示。
   - **判断方法**：
     - 包括但不限于性行为、性器官描述、性暗示、性挑逗等内容。
     - 内容中包含低俗、露骨的性描写，或者明确提到与性有关的行为
   - **相关内容示例**：
     - 性交易、色情内容、裸露图片或视频、性骚扰描述等。

---

### 2. **harassment（骚扰）**

   - **定义**：表示内容是否涉及对他人进行骚扰或侵犯的行为。
   - **判断方法**：
     - 包括但不限于言语骚扰、网络暴力、跟踪威胁等内容。
     - 内容中包含针对特定个人的侮辱性语言、威胁性言论，或者反复纠缠他人的行为
   - **相关内容示例**：
     - “别找我，不然我会对你做些坏事。”
     - 针对某人发送大量垃圾信息或重复性攻击内容。

---

### 3. **hate（仇恨言论）**

   - **定义**：表示内容是否包含仇恨、歧视或贬低特定群体的言论。
   - **判断方法**：
     - 包括但不限于针对种族、民族、宗教、性别、性取向等的歧视性言论。
     - 内容中包含贬低、攻击或威胁某个群体的内容
   - **相关内容示例**：
     - “黑人都是懒惰的。”
     - “同性恋不应该被允许结婚。”

---

### 4. **illicit（非法内容）**

   - **定义**：表示内容是否涉及任何违反法律法规的行为或活动。
   - **判断方法**：
     - 包括但不限于赌博、贩毒、洗钱、伪造证件、盗窃等违法行为的描述或宣扬。
     - 内容中包含教唆他人犯罪或详细描述非法行为
   - **相关内容示例**：
     - “这里有如何制作假钞的方法，有兴趣吗？”
     - 宣扬赌博平台或贩毒信息。

---

### ~~5. **self-harm（自残行为）**~~

   - ~~**定义**：表示内容是否涉及鼓励、描述或美化自残、自我伤害的行为。~~
   - ~~**判断方法**：~~
     - ~~包括但不限于割腕、烧伤、过度节食等自残行为的详细描述或鼓励他人效仿的内容。~~
     - ~~内容中包含诱导他人进行自残行为，或者分享自残经历且可能对他人造成负面影响~~
   - ~~**相关内容示例**：~~
     - ~~“我最近经常割腕，感觉很爽。”~~
     - ~~分享自残工具或方法。~~

*AI不支持判断该类型, AI判断不出来

---

### 6. **violence（暴力）**

   - **定义**：表示内容是否涉及任何形式的暴力行为或极端言辞。
   - **判断方法**：
     - 包括但不限于对他人身体伤害、恐怖主义活动、战争场景等的描述。
     - 如果内容中包含详细描述暴力行为，或者鼓吹暴力，则判定为 `true`。
   - **相关内容示例**：
     - “我今天杀了一个人。”
     - 描述爆炸、枪击等暴力事件的具体细节。

### **7. political（政治性）**

- **定义**：
  - 表示内容是否涉及政治相关的话题、事件或人物，尤其是与各国国家领导人、政府政策、政党活动等相关的内容。
- **判断方法**：
  - 如果内容中直接提到某国的现任或前任领导人，并且讨论与其相关的政治话题
  - 提到特定政党的名称、活动、宣传或争议事件。
  - 涉及重大政治事件，如选举、罢工、抗议活动等。
  - 讨论分裂国家、颠覆政权、恐怖主义等具有高度敏感性的内容。
- **相关内容示例**：
  - “我对现任总统的某些决策感到不满。”
  - 宣扬分离主义思想或攻击某个政治体制的内容。
  - 提到习近平、拜登、普京等领导人的相关内容。
