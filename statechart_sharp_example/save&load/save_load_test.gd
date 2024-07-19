class_name StatechartSaveLoadTest

extends Node


var _statechart: Statechart
var _snapshot: StatechartSnapshot


func _ready():
	_statechart = get_node_or_null("Statechart") as Statechart
	_snapshot = _statechart.Save(true)
	_statechart.Load(_snapshot, true, true)

