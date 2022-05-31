# Copyright 2015 gRPC authors.
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
"""The Python implementation of the gRPC route guide server."""

from concurrent import futures
import logging
from threading import Lock
import os, os.path
import random
import grpc
import Battle2DServer_pb2
import Battle2DServer_pb2_grpc


STORE_DIR = "C:/Users/Steinar/AppData/LocalLow/Nerhus/Battle2D/"

def get_dir(round_no):
    return STORE_DIR + "round" + str(round_no) + "/"

def save_player(player : Battle2DServer_pb2.PlayerData) -> bool:
    dir = get_dir(player.round)
    if not os.path.isdir(dir):
        os.mkdir(dir)
    n = len(os.listdir(dir))
    player_file = open(dir + "p" + str(n), "wb+")
    player_file.write(player.serialized_player_data)
    player_file.close()
    return True

def load_player(request : Battle2DServer_pb2.LoadRequest) -> Battle2DServer_pb2.PlayerData:
    round = request.round
    dir = get_dir(round)
    n = len(os.listdir(get_dir(round)))
    x = random.randint(0, n-1)
    player_file = open(get_dir(round) + "p" + str(x), "rb")

    player = Battle2DServer_pb2.PlayerData()
    player.round = 0
    player.serialized_player_data = player_file.readline()

    player_file.close()

    return player

class DataStorageServicer(Battle2DServer_pb2_grpc.DataStorageServicer):
    """Provides methods that implement functionality of data storage server."""

    def __init__(self):
        self.lock = Lock()

    def SavePlayer(self, request, context):
        self.lock.acquire()
        print("save player") 
        ok = save_player(request)
        self.lock.release()
        return Battle2DServer_pb2.SaveResponse()

    def LoadPlayer(self, request, context):
        self.lock.acquire()
        print("load player") 
        player = load_player(request)
        self.lock.release()
        return player


def serve():
    print("Starting Server") 
    server = grpc.server(futures.ThreadPoolExecutor(max_workers=10))
    Battle2DServer_pb2_grpc.add_DataStorageServicer_to_server(
        DataStorageServicer(), server)
    server.add_insecure_port('[::]:50124')
    server.start()
    server.wait_for_termination()


if __name__ == '__main__':
    logging.basicConfig()
    random.seed()
    serve()