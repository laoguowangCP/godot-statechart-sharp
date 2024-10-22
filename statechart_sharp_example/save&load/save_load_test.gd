extends Node


@export var is_all_state_config: bool
@export var enter_on_load: bool
@export var exit_on_load: bool

var _statechart: Statechart
var _snapshot: StatechartSnapshot
var _save_path: String = "res://statechart_sharp_example/save&load/statechart.tres"


func _ready() -> void:
	_statechart = get_node_or_null("Statechart") as Statechart
	connect_to_test(_statechart)
	print(%BoxContainer.name)


func connect_to_test(node: Node) -> void:
	for child in node.get_children():
		if child is State:
			child.Enter.connect(OnStateEnter)
			child.Exit.connect(OnStateExit)
		elif child is Transition:
			child.Guard.connect(OnTransitionGuard)
			child.Invoke.connect(OnTransitionInvoke)
		elif child is Reaction:
			child.Invoke.connect(OnActionInvoke)
		else:
			pass

		connect_to_test(child)


func OnTransitionGuard(duct: StatechartDuct) -> void:
	PrintDelegateInfo(duct, "(Guard)")


func OnTransitionInvoke(duct: StatechartDuct) -> void:
	PrintDelegateInfo(duct, "(Invoke)")


func OnActionInvoke(duct: StatechartDuct) -> void:
	PrintDelegateInfo(duct, "(Invoke)")


func OnStateEnter(duct: StatechartDuct) -> void:
	PrintDelegateInfo(duct, "(Enter)")


func OnStateExit(duct: StatechartDuct) -> void:
	PrintDelegateInfo(duct, "(Exit)")



func PrintDelegateInfo(duct: StatechartDuct, delegateName: String) -> void:
	var compositionNode = duct.CompositionNode as Node
	var path = get_path_to(compositionNode);
	var indentStr = ""
	for i in path.get_name_count():
		indentStr = indentStr + " — "
	print(indentStr, delegateName, " ", path);


func _go() -> void:
	print(">> Save ")
	_snapshot = _statechart.Save(is_all_state_config)
	ResourceSaver.save(_snapshot, _save_path)
	print(">> Sudo step")
	_statechart.Step("sudo_step")
	print(">> Load ")
	_snapshot = ResourceLoader.load(_save_path) as StatechartSnapshot
	_statechart.Load(_snapshot, exit_on_load, enter_on_load)
	print("Statechart configuration loaded.")

