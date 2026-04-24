# 《深潜构筑》Inspector 配置记录

## GameScene Hierarchy
- Main Camera
- Canvas
  - EventSystem
  - Text (TMP)
  - Btn_ToResult
    - Text (TMP)
- SceneLoader
- TestTarget
- DamageZone
- Player
- Enemy_Chaser
  - AttackRange

---

## Player

### 基础
- Name = Player
- Tag = Player
- Layer = Player

### 组件
- Transform
- SpriteRenderer
- Rigidbody2D
- BoxCollider2D
- PlayerController
- PlayerAttack
- Health
- PlayerDeathHandler

### Rigidbody2D
- Body Type = Dynamic
- Simulated = true
- Mass = 1
- Linear Drag = 0
- Angular Drag = 0.05
- Gravity Scale = 0
- Collision Detection = Continuous
- Constraints:
  - Freeze Rotation Z = true

### BoxCollider2D
- Material = PM_NoFriction
- Is Trigger = false
- Offset = (0, 0)
- Size = (1, 1)

### PlayerController
- Move Speed = 50
- Dash Speed = 150
- Dash Duration = 0.2
- Dash Cooldown = 1

### PlayerAttack
- Projectile Prefab = WaterBullet
- Attack Cooldown = 0.3
- Spawn Offset = 20

### Health
- Max Hp = 5
- Current Hp = 5
- Use Invincible = true
- Invincible Time = 1
- Flash On Hit = true
- Flash Interval = 0.1
- Destroy On Death = false
- Disable On Death = true

---

## Enemy_Chaser

### 基础
- Name = Enemy_Chaser
- Tag = Untagged
- Layer = Default

### 组件
- Transform
- SpriteRenderer
- Rigidbody2D
- Health
- EnemyChaser
- CircleCollider2D

### Rigidbody2D
- Body Type = Dynamic
- Simulated = true
- Mass = 1
- Linear Drag = 0
- Angular Drag = 0.05
- Gravity Scale = 0
- Collision Detection = Continuous
- Constraints:
  - Freeze Rotation Z = true

### Health
- Max Hp = 3
- Current Hp = 3
- Use Invincible = false
- Invincible Time = 1
- Flash On Hit = true
- Flash Interval = 0.1
- Destroy On Death = true
- Disable On Death = false

### EnemyChaser
- Player Target = Player
- Move Speed = 15
- Detect Range = 350
- Stop Distance = 26

### CircleCollider2D
- Material = PM_NoFriction
- Is Trigger = false
- Offset = (0, 0)
- Radius = 0.38

---

## AttackRange

### 基础
- Name = AttackRange
- Parent = Enemy_Chaser
- Tag = Untagged
- Layer = Default

### 组件
- Transform
- CircleCollider2D
- EnemyAttackRange

### CircleCollider2D
- Material = None
- Is Trigger = true
- Offset = (0, 0)
- Radius = 0.68

### EnemyAttackRange
- Damage = 1
- Damage Cooldown = 1

---

## WaterBullet

### 基础
- Name = WaterBullet

### 组件
- Transform
- SpriteRenderer
- CircleCollider2D
- Projectile
- Rigidbody2D

### CircleCollider2D
- Material = None
- Is Trigger = true
- Offset = (0, 0)
- Radius = 0.5
- Exclude Layers = Player

### Projectile
- Speed = 65.14
- Life Time = 2
- Damage = 1

### Rigidbody2D
- Body Type = Dynamic
- Simulated = true
- Mass = 1
- Linear Drag = 0
- Angular Drag = 0.05
- Gravity Scale = 0
- Collision Detection = Continuous
- Freeze Rotation Z = true

---

## DamageZone

### 当前状态
- 场景中存在
- 当前更像测试对象
- 正式战斗逻辑不要默认依赖它

### 备注
- DamageZone.cs 目前是 OnTriggerEnter2D 扣一次血
- 不是持续伤害区

---

## 待补记录
- Build Settings 场景列表
- WaterBullet 所在 Layer / Tag
- TestTarget 的挂载与用途