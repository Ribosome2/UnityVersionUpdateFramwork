# UnityVersionUpdateFramwork
Unity 资源更新框架
机制：  
  1.StreammingAssets下面跟正式发布包一起出去，发布的时候会用工具生成一份文件列表信息filelistOriginal.txt(文件类型可以自己定)
    信息可以包括文件名， MD5码...
  
  2.要打补丁更新的时候，资源服务器上面也会有一份filelist.txt 记录的内容为跟正式包内容对比，到目前位置要更新的所有文件列表信息
  
  3.Unity 里面的代码流程，
     1）下载远端服务器的fileList 
     2)  检测persistentPath下是否有filelistLocal文件，如果没有，就创建到这个目录下，因为streammingAssets 路径是只读的，
        之后我们读写都将用这个新创建的文件
     3） 加载（2）步骤中的filelistLocal 和fileListOriginal, 在缓存中创建一个文件列表temp-->将Orinal的列表复制进这个文件列表temp-->
          遍历fileLocal,替换同名的文件信息或者新增文件信息到temp,
          这里我们进得到的是: 原包文件列表+已经更新过的文件列表=本地所有的文件列表
          （这里有可能根据文件列表检测本地是否确实存在指定文件，而且没有损坏，耗点计算，看情况）
     4） 用temp对比（1） 下载的文件fileList，如果temp列表没有的直接添加到更新列表，
         如果已经有的，对比MD5或者其他特征， 跟服务器的不一致就添加到更新列表
        
     5）根据更新列表把远端资源下载到persistentPath目录，本地已经有的话就进行覆盖，
        下载完成一个就更新一下本地的fileListLocal
     
  
  完成上面的步骤之后， 我们的资源已经全部是最新的，可以根据filelistLocal 创建一个已经更新过的文件列表，加载资源的时候
  根据要加载的资源名判断，如果资源已经是更新过的，就加载persistentPath目录下的资源， 如果不存在就加载streammingAssets下的资源
  
  是不是感觉这套机制有个弊病，只要资源更新过，被更新的资源实际上就有了两套，而且streammingAssets下的那一份就成了死资源？
  确实，所以我想这套机制还是做分包发布的做法，（streammingAsset资源不跟正式包出去，启动的时候下载（做压缩，下载之后解压最好））
  
