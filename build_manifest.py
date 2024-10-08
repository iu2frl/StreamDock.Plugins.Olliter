import sys
import json
import re

# Define the paths for the .cs file and manifest.json file
cs_file_path = 'src/PluginAction.cs'
manifest_file_path = 'it.iu2frl.streamdock.olliter.sdPlugin/manifest.json'

# Regular expressions to capture Action details from the .cs file
action_name_regex = r'// Name:\s*(.+)'
tooltip_regex = r'// Tooltip:\s*(.+)'
uuid_regex = r'\[PluginActionId\("(.+)"\)\]'

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
        name_match = re.search(action_name_regex, line)
        if name_match:
            current_action['Name'] = name_match.group(1)

        # Check for Tooltip comment
        tooltip_match = re.search(tooltip_regex, line)
        if tooltip_match:
            current_action['Tooltip'] = tooltip_match.group(1)

        # Check for PluginActionId attribute
        uuid_match = re.search(uuid_regex, line)
        if uuid_match:
            current_action['UUID'] = uuid_match.group(1)

        # When all required fields are found, append action and reset current_action
        if 'Name' in current_action and 'Tooltip' in current_action and 'UUID' in current_action:
            # Default values for Icon and Controllers
            current_action['Icon'] = "images/Olliter"
            current_action['Controllers'] = ["Keypad", "Information"]
            current_action['SupportedInMultiActions'] = False

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
    actions = extract_actions_from_cs(cs_file_path)
    
    # Update the manifest with the extracted actions
    update_manifest(manifest_file_path, actions)

    print("Manifest updated successfully.")

if __name__ == "__main__":
    main()
    sys.exit(0)
