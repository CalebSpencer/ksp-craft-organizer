﻿using System;
using System.Collections.Generic;

namespace KspCraftOrganizer {

	public class OrganizerServiceFilterGroupsOfTagModel {

		private OrganizerService parent;
		private FilterTagsGrouper tagsGrouper;
		private Dictionary<string, string> groupsWithSelectedNone = new Dictionary<string, string>();

		public OrganizerServiceFilterGroupsOfTagModel(OrganizerService parent) {
			this.tagsGrouper = new FilterTagsGrouper(parent);
			this.parent = parent;
		}


		public void update() {
			ICollection<OrganizerTagModel> usedTags = parent.usedTags;
			this.tagsGrouper.update(usedTags);

			List<string> groupsToRemove = new List<string>();
			foreach (string g in groupsWithSelectedNone.Keys) {
				if (!tagsGrouper.groupExists(g)) {
					groupsToRemove.Add(g);
				}
			}
			foreach (string g in groupsToRemove) {
				groupsWithSelectedNone.Remove(g);
			}
		}

		public ICollection<FilterTagGroup> groups {
			get {
				return this.tagsGrouper.groups;
			}
		}

		public ICollection<OrganizerTagModel> restTags {
			get {
				return this.tagsGrouper.restTags;
			}
		}

		public bool restGroupCollapsed { get; set; }

		public ICollection<string> collapsedFilterGroups {
			get {
				List<string> toRet = new List<string>();
				foreach (FilterTagGroup tagGroup in tagsGrouper.groups) {
					if (tagGroup.collapsedInFilterView) {
						toRet.Add(tagGroup.name);
					}
				}
				return toRet;
			}
		}

		public void setInitialGroupsWithSelectedNone(ICollection<string> filterGroupsWithSelectedNoneOption) {
			groupsWithSelectedNone.Clear();
			foreach (string groupName in filterGroupsWithSelectedNoneOption) {
				if (!groupsWithSelectedNone.ContainsKey(groupName)) {
					groupsWithSelectedNone.Add(groupName, groupName);
				}
			}
		}

		public void setCollapsedGroups(ICollection<string> collapsedGroups) {
			foreach (FilterTagGroup tagGroup in tagsGrouper.groups) {
				tagGroup.collapsedInFilterView = collapsedGroups.Contains(tagGroup.name);
			}
		}

		public ICollection<string> groupsWithSelectedNoneOption {
			get {
				return groupsWithSelectedNone.Keys;
			}
		}

		public void clearFilters() {
			groupsWithSelectedNone.Clear();
		}

		public void setGroupHasSelectedNoneFilter(string groupName, bool isNoneFilterSelected) {
			if (isNoneFilterSelected) {
				if (!groupsWithSelectedNone.ContainsKey(groupName)) {
					groupsWithSelectedNone.Add(groupName, groupName);
					parent.markFilterAsChanged();
				}
			} else {
				if (groupsWithSelectedNone.ContainsKey(groupName)) {
					groupsWithSelectedNone.Remove(groupName);
					parent.markFilterAsChanged();
				}
			}
		}

		public bool hasGroupSelectedNoneFilter(string groupName) {
			return groupsWithSelectedNone.ContainsKey(groupName);
		}

		public bool doesCraftPassFilter(OrganizerCraftModel craft, out bool shouldBeVisibleByDefault) {
			if (tagsGrouper == null) {
				COLogger.logError("tagsGrouper == null!");
				shouldBeVisibleByDefault = false;
				return false;
			}
			bool pass = true;
			shouldBeVisibleByDefault = true;
			foreach (TagGroup<OrganizerTagModel> tagGroup in tagsGrouper.groups) {
				bool anythingSelectedInThisGroup = false;
				bool craftPassesAnythingInThisGroup = false;
				bool craftContainsAnyTagFromThisGroup = false;
				foreach (TagInGroup<OrganizerTagModel> tag in tagGroup.tags) {
					bool craftHasThisTag = craft.containsTag(tag.originalTag.name);
					craftContainsAnyTagFromThisGroup = craftContainsAnyTagFromThisGroup || craftHasThisTag;
					if (tag.originalTag.selectedForFiltering) {
						anythingSelectedInThisGroup = true;
						craftPassesAnythingInThisGroup = craftPassesAnythingInThisGroup || craftHasThisTag;
					}

					if (YesNoTag.isByDefaultNegativeTag(tag.originalTag.name) && craft.containsTag(tag.originalTag.name)) {
						shouldBeVisibleByDefault = false;
					}
					if (YesNoTag.isByDefaultPositiveTag(tag.originalTag.name) && !craft.containsTag(tag.originalTag.name)) {
						shouldBeVisibleByDefault = false;
					}

				}
				if (hasGroupSelectedNoneFilter(tagGroup.name)) {
					anythingSelectedInThisGroup = true;
					craftPassesAnythingInThisGroup = craftPassesAnythingInThisGroup || !craftContainsAnyTagFromThisGroup;
				}
				if (anythingSelectedInThisGroup && !craftPassesAnythingInThisGroup) {
					pass = false;
					break;
				}

			}

			foreach (OrganizerTagModel tag in tagsGrouper.restTags) {
				if (tag.selectedForFiltering) {
					if (!craft.containsTag(tag.name)) {
						pass = false;
					}
				}	
			}

			return pass;
		}
	}
}

