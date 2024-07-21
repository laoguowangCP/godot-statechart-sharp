class_name StatechartSaveLoadTest

extends Node


var _statechart_1: Statechart
var _statechart_2: Statechart
var _snapshot: StatechartSnapshot


func _test():
	_statechart_1 = get_node_or_null("Statechart1") as Statechart
	_statechart_2 = get_node_or_null("Statechart2") as Statechart
	_snapshot = _statechart_1.Save(true)
	_statechart_2.Load(_snapshot, true, true)
	print("Statechart configuration loaded.")

