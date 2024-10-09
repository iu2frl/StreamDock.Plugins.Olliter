"""
Generation of the manifest.json at every compilation
"""

import sys
import json
import re

# Define the paths for the .cs file and manifest.json file
CS_FILE_PATH = 'src/PluginAction.cs'
MANIFEST_FILE_PATH = 'it.iu2frl.streamdock.olliter.sdPlugin/manifest.json'

# Regular expressions to capture Action details from the .cs file
ACTION_NAME_REGEX = r'// Name:\s*(.+)'
TOOLTIP_REGEX = r'// Tooltip:\s*(.+)'
CONTROLLERS_REGEX = r'// Controllers:\s*(.+)'
UUID_REGEX = r'\[PluginActionId\("(.+)"\)\]'

def extract_actions_from_cs(cs_file):
    """
    Extracts action details from a .cs file by scanning for specific comments
    and attributes that describe each action.

    Args:
        cs_file (str): The path to the .cs file.

    Returns:
        list: A list of dictionaries, where each dictionary contains information
              about an action (Name, Tooltip, UUID, Icon, Controllers, SupportedInMultiActions).
    """
    actions = []
    with open(cs_file, 'r', encoding="utf-8") as file:
        lines = file.readlines()

    current_action = {}
    for line in lines:
        # Check for Name comment
        name_match = re.search(ACTION_NAME_REGEX, line)
        if name_match:
            current_action['Name'] = name_match.group(1)

        # Check for Tooltip comment
        tooltip_match = re.search(TOOLTIP_REGEX, line)
        if tooltip_match:
            current_action['Tooltip'] = tooltip_match.group(1)

        # Check for Controllers comment
        controllers_match = re.search(CONTROLLERS_REGEX, line)
        if controllers_match:
            # Split by comma, remove any leading/trailing spaces, and form the list
            controllers = [
                controller.strip() for controller in controllers_match.group(1).split(',')
            ]
            current_action['Controllers'] = controllers

        # Check for PluginActionId attribute
        uuid_match = re.search(UUID_REGEX, line)
        if uuid_match:
            current_action['UUID'] = uuid_match.group(1)

        # When all required fields are found, append action and reset current_action
        if 'Name' in current_action and 'Tooltip' in current_action and 'UUID' in current_action:
            # Default values for Icon and Controllers
            current_action['Icon'] = "images/Olliter"
            current_action['SupportedInMultiActions'] = True

            actions.append(current_action)
            current_action = {}  # Reset for next action

    return actions

def update_manifest(manifest_file, new_actions):
    """
    Updates the manifest.json file with a new set of actions.

    Args:
        manifest_file (str): The path to the manifest.json file.
        new_actions (list): A list of new actions to be written to the manifest.

    Returns:
        None: The function updates the file in-place and does not return anything.
    """
    with open(manifest_file, 'r', encoding="utf-8") as file:
        manifest_data = json.load(file)

    # Replace the actions in the manifest with the new actions
    manifest_data['Actions'] = new_actions

    # Save the updated manifest.json
    with open(manifest_file, 'w', encoding="utf-8") as file:
        json.dump(manifest_data, file, indent=4)

def main():
    """
    Main function that handles the process of extracting action details
    from a .cs file and updating the manifest.json file with the new actions.

    Returns:
        None: The function runs the entire workflow and outputs success messages.
    """
    # Extract actions from the .cs file
    actions = extract_actions_from_cs(CS_FILE_PATH)

    # Update the manifest with the extracted actions
    update_manifest(MANIFEST_FILE_PATH, actions)

    print("Manifest updated successfully.")

if __name__ == "__main__":
    main()
    sys.exit(0)
