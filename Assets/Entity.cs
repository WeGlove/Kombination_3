using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    float x = 0;
    float z = 0;
    float height = 0;
    public Type type;
    
    public int grown_tick = 0;
    
    public float MINIMUM_WATER_GRASS = 1f / Mathf.Pow(2,5);
    public float MINIMUM_WATER_CACTUS = 1f / Mathf.Pow(2,9);
    public float MAXIMUM_WATER_CACTUS = 1f / Mathf.Pow(2,5);
    
    public enum Type{
        SEED,
        GRASS,
        REED,
        CACTUS,
        COMPOST,
        FLOWER
    }

    // Start is called before the first frame update
    void Start()
    {
    }
    
    public void set_type(Type new_type){
        this.type = new_type;
        Debug.Log(this.type);
        this.reset_model();
    }
    
    public void reset_model(){
        switch (this.type){
            case Type.SEED:
                this.GetComponent<Renderer>().material.color = new Color(0.8f, 1f, 0.8f);
                gameObject.transform.localScale = new Vector3(0.9f,0.9f,0.9f);
                this.transform.position = new Vector3(this.x, 0.5f * (this.height-1) + 1, this.z);
                break;
            case Type.GRASS:
                this.GetComponent<Renderer>().material.color = new Color(0.1f, 0.6f, 0.1f);
                gameObject.transform.localScale = new Vector3(0.9f,0.1f,0.9f);
                this.transform.position = new Vector3(this.x, 0.5f * (this.height-1f) + 0.6f, this.z);
                break;
            case Type.REED:
                this.GetComponent<Renderer>().material.color = new Color(0.05f, 0.3f, 0.1f);
                gameObject.transform.localScale = new Vector3(0.9f,0.2f,0.9f);
                this.transform.position = new Vector3(this.x, 0.5f * (this.height-1f) + 0.7f, this.z);
                break;
            case Type.CACTUS:
                this.GetComponent<Renderer>().material.color = new Color(0.2f, 0.5f, 0.1f);
                gameObject.transform.localScale = new Vector3(0.7f,2f,0.7f);
                this.transform.position = new Vector3(this.x, 0.5f * (this.height-1f) + 1.7f, this.z);
                break;
            case Type.COMPOST:
                this.GetComponent<Renderer>().material.color = new Color(0.2f, 0.1f, 0.1f);
                gameObject.transform.localScale = new Vector3(0.9f,0.1f,0.9f);
                this.transform.position = new Vector3(this.x, 0.5f * (this.height-1f) + 0.6f, this.z);
                break;
            case Type.FLOWER:
                this.GetComponent<Renderer>().material.color = new Color(0.9f, 0.6f, 0.6f);
                gameObject.transform.localScale = new Vector3(0.2f,0.9f,0.2f);
                this.transform.position = new Vector3(this.x, 0.5f * (this.height-1f) + 0.6f, this.z);
                break;
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void set_position(float x, float z, int height){
        this.x = x;
        this.z = z;
        
        this.set_height(height);
    }
    
    public void set_height(int new_height){
        this.height = new_height;
        this.reset_model();
    }
    
    public void act(Game game, int x, int y){
        Cell cell = game.cells[x,y].GetComponent<Cell>();
        if (this.grown_tick != game.ticks)
            switch(type){
                case Entity.Type.SEED:
                    this.act_seed(game, cell);
                    break;
                case Entity.Type.GRASS:
                    this.act_grass(game, cell, x, y);
                    break;
                case Entity.Type.REED:
                    this.act_reed(game, cell, x, y);
                    break;
                case Entity.Type.CACTUS:
                    this.act_cactus(game, cell, x, y);
                    break;
                case Entity.Type.COMPOST:
                    this.act_compost(game, cell, x, y);
                    break;
                default:
                    Debug.Log("Tobi fucked up!");
                    break;
            }
    }
    
    void act_seed(Game game, Cell cell){
        if (cell.water)
            cell.remove_entity();
        else{
            if (cell.water_level > this.MINIMUM_WATER_CACTUS && cell.water_level < this.MAXIMUM_WATER_CACTUS) 
                this.set_type(Entity.Type.CACTUS);
            else if(cell.water_level > this.MINIMUM_WATER_GRASS){
                this.set_type(Entity.Type.GRASS);
            }
            else 
                cell.remove_entity();
        }

    }
    
    void act_grass(Game game, Cell cell, int i, int j){
        if (cell.water_level < this.MINIMUM_WATER_GRASS || cell.water)
            cell.entity.GetComponent<Entity>().set_type(Type.COMPOST);
        else {
            int x = Random.Range(-1,2);
            int y = Random.Range(-1,2);

            if (!(x ==0 && y == 0) && game.is_coord_inside(x+i,y+j)){
                Cell grow_cell = game.cells[x+i,y+j].GetComponent<Cell>();
                if(!grow_cell.water && grow_cell.water_level >= this.MINIMUM_WATER_GRASS && grow_cell.entity == null){
                    grow_cell.seed(Entity.Type.GRASS, game.ticks);
                    Entity grass = grow_cell.entity.GetComponent<Entity>();
                    grass.grown_tick = game.ticks;                
                }
            }
            
            if (game.next_to_water(i, j, 2)){
                this.set_type(Entity.Type.REED);
                this.grown_tick = game.ticks;
            } else if (game.next_to_type(i, j, Entity.Type.COMPOST)){
                this.set_type(Entity.Type.FLOWER);
            }
        }
    }
    
    void act_reed(Game game, Cell cell, int i, int j){
        if (cell.water_level < this.MINIMUM_WATER_GRASS || cell.water)
            cell.entity.GetComponent<Entity>().set_type(Type.COMPOST);
        else {
            int x = Random.Range(-1,2);
            int y = Random.Range(-1,2);

            if (!(x ==0 && y == 0) && game.is_coord_inside(x+i,y+j)){
                Cell grow_cell = game.cells[x+i,y+j].GetComponent<Cell>();
                if(!grow_cell.water && grow_cell.water_level >= this.MINIMUM_WATER_GRASS && grow_cell.entity == null){
                    if (game.next_to_water(x+i,y+j, 2))
                        grow_cell.seed(Entity.Type.REED, game.ticks);
                    else
                        grow_cell.seed(Entity.Type.GRASS, game.ticks);
                    Entity grass = grow_cell.entity.GetComponent<Entity>();
                    grass.grown_tick = game.ticks;                
                }
            }
        }
    }
    
    void act_cactus(Game game, Cell cell, int i, int j){
        if (cell.water_level > this.MINIMUM_WATER_GRASS || cell.water_level < this.MINIMUM_WATER_CACTUS)
            cell.entity.GetComponent<Entity>().set_type(Type.COMPOST);
        else {
            int x = Random.Range(-2,3);
            int y = Random.Range(-2,3);
            if ((Mathf.Abs(x) == 2 || Mathf.Abs(y) == 2) && game.is_coord_inside(i+x,j+y)){
                Cell grow_cell = game.cells[x+i,y+j].GetComponent<Cell>();
                if(!grow_cell.water && grow_cell.water_level > this.MINIMUM_WATER_CACTUS && grow_cell.water_level < this.MAXIMUM_WATER_CACTUS && grow_cell.entity == null && !game.next_to_type(i+x, j+y, Type.CACTUS)){
                    grow_cell.seed(Type.CACTUS, game.ticks);
                }
            }
        }
    }
    
    void act_compost(Game game, Cell cell, int i, int j){
    }
}
