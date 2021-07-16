using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    
    public Camera cam;
    public GameObject[,] cells;
    public int width = 1;
    public int height = 1;
    public Material cell_material;
    public GameObject sun_light;
    public GameObject moon_light;
    private int selection_x = 0;
    private int selection_y = 0;
    private float cam_move_speed = 2f;
    private float last_tick;
    public int ticks = 0;
    public float tick_speed = 1;
    
    private int SUN_CYCLE_TICKS = 100;
    private float WATER_SPREAD = 1.7f;

    // Start is called before the first frame update
    void Start()
    {
        this.last_tick = Time.time;
    
        this.cells = new GameObject[this.width,this.height];
        for (int i=0; i < this.width; i++){
            for (int j=0; j < this.height; j++){
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.name = "Cell " + i + " " + j;
                cube.AddComponent<Cell>();
                cube.transform.position = new Vector3(i,0,j);
                cube.GetComponent<Renderer>().material = cell_material;
                
                this.cells[i,j] = cube;
            }
        }
        this.randomize_terrain_sin(0.1f);
    }

    // Update is called once per frame
    void Update()
    {
        this.move_camera();
        this.change_selection();
        this.change_height();
        this.change_water();
        this.change_seed();
        this.remove();
        
        if (Time.time > this.last_tick + this.tick_speed){
            this.last_tick = Time.time;
            this.tick();
        }
    }
    
    void change_height(){
        if (Input.GetKeyDown("q")){
            this.cells[this.selection_x, this.selection_y].GetComponent<Cell>().inc_height();
            this.redistribute_water();
            this.recalculate_light();
        }

        if (Input.GetKeyDown("e")){
            this.cells[this.selection_x, this.selection_y].GetComponent<Cell>().dec_height();
            this.redistribute_water();
            this.recalculate_light();
        }

    }
    
    void move_camera(){
        float move_x = 0f;
        float move_z = 0f;
        
        if (Input.GetKey("right")){
            move_x += this.cam_move_speed * Time.deltaTime;
        }
        if (Input.GetKey("left")){
            move_x -= this.cam_move_speed * Time.deltaTime;
        }
        if (Input.GetKey("up")){
            move_z += this.cam_move_speed * Time.deltaTime;
        }
        if (Input.GetKey("down")){
            move_z -= this.cam_move_speed * Time.deltaTime;
        }
        
        this.cam.transform.position += new Vector3(move_x,0, move_z);
    }
    
    void change_selection(){
        this.cells[this.selection_x, this.selection_y].GetComponent<Cell>().unselected();
        if (Input.GetKeyDown("d")){
            if (this.selection_x < this.width-1)
                this.selection_x ++;
        }
        if (Input.GetKeyDown("a")){
            if (this.selection_x > 0)
                this.selection_x --;
        }
        if (Input.GetKeyDown("w")){
            if (this.selection_y < this.height-1)
                this.selection_y ++;
        }
        if (Input.GetKeyDown("s")){
            if (this.selection_y > 0)
                this.selection_y --;
        }
        this.cells[this.selection_x, this.selection_y].GetComponent<Cell>().selected();
    }
    
    void change_water(){
        if (Input.GetKeyDown("r")){
            this.cells[this.selection_x, this.selection_y].GetComponent<Cell>().create_water();
            this.redistribute_water();
        }
    }
    
    void redistribute_water(){
        this.reset_water_levels();
        for (int i=0; i < this.width; i++)
            for (int j=0; j < this.height; j++)
                if (this.cells[i, j].GetComponent<Cell>().water)
                    this.compute_water_for_block(i, j);
        this.reset_all_color();
    }

    void recalculate_light(){
        for (int i=0; i < this.width; i++)
            for (int j=0; j < this.height; j++)
                this.recalculate_light_for_block(i,j);
        this.reset_all_color();
    }

    void recalculate_light_for_block(int i, int j){
        
        int light_height = this.cells[i,j].GetComponent<Cell>().height;
        float light = 1;
        
        if (this.is_coord_inside(i-1, j))  
            if (this.cells[i-1,j].GetComponent<Cell>().height > light_height)
                light -= 0.5f;
        
        if (this.is_coord_inside(i+1, j))  
            if (this.cells[i+1,j].GetComponent<Cell>().height > light_height)
                light -= 0.5f;
        
        this.cells[i,j].GetComponent<Cell>().light_level = light;
    }
    
    void reset_water_levels(){
        for (int i=0; i < this.width; i++)
            for (int j=0; j < this.height; j++)
                this.cells[i, j].GetComponent<Cell>().water_level = 0;
    }
    
    void compute_water_for_block(int x, int y){
        for (int i=0; i < this.width; i++)
            for (int j=0; j < this.height; j++){
                float given_level = this.cells[i, j].GetComponent<Cell>().water_level;
                int difference = Mathf.Abs(i-x) + Mathf.Abs(j-y);
                int height_diff = this.cells[i, j].GetComponent<Cell>().height - this.cells[x, y].GetComponent<Cell>().height;
                if (height_diff > 0)
                    difference += height_diff;
                float counter_level = 1 / Mathf.Pow(this.WATER_SPREAD, difference);
                if (counter_level >given_level)
                    this.cells[i, j].GetComponent<Cell>().water_level = counter_level;
            }
    }
    
    void reset_all_color(){
        for (int i=0; i < this.width; i++)
            for (int j=0; j < this.height; j++)
                this.cells[i, j].GetComponent<Cell>().reset_color();
    }
    
    void change_seed(){
        if (Input.GetKeyDown("z")){
            this.cells[this.selection_x, this.selection_y].GetComponent<Cell>().seed(this.ticks);
        }
    }
    
    void remove(){
        if (Input.GetKeyDown("t")){
            if (this.cells[this.selection_x, this.selection_y].GetComponent<Cell>().water){
                this.cells[this.selection_x, this.selection_y].GetComponent<Cell>().destroy_water();
                this.redistribute_water();
            }
            this.cells[this.selection_x, this.selection_y].GetComponent<Cell>().remove_entity();
        }
    }
    
    void tick(){
        this.ticks++;
        Debug.Log("------TICK------");
        for (int i=0; i < this.width; i++)
            for (int j=0; j < this.height; j++){
                GameObject cell = this.cells[i, j];
                cell.GetComponent<Cell>().act(this, i, j);                  
            }
        
        this.sun_light.transform.rotation  =  this.sun_light.transform.rotation * Quaternion.Euler(360/this.SUN_CYCLE_TICKS,0,0);
        this.moon_light.transform.rotation  =  this.moon_light.transform.rotation * Quaternion.Euler(360/this.SUN_CYCLE_TICKS,0,0);

    }
    
    public bool is_coord_inside(int x, int y){
        return x >= 0 && x < this.width && y >= 0 && y < this.height;
    }
    
    public bool next_to_water(int x, int y, int distance){
        for (int i=-distance; i < distance+1; i++)
            for (int j=-distance; j < distance+1; j++)
                if (!(i==0 && j==0) && this.is_coord_inside(x+i,y+j))
                    if (this.cells[x+i, y+j].GetComponent<Cell>().water)
                        return true;
        return false;
    }
    
    public bool next_to_water(int x, int y){
        return this.next_to_water(x, y, 0);
    }
    
    public bool next_to_type(int x, int y, Entity.Type type){
        for (int i=-1; i < 2; i++)
            for (int j=-1; j < 2; j++)
                if (!(i==0 && j==0) && this.is_coord_inside(x+i,y+j))
                    if (this.cells[x+i, y+j].GetComponent<Cell>().entity != null)
                        if (this.cells[x+i, y+j].GetComponent<Cell>().entity.GetComponent<Entity>().type == type)
                            return true;
        return false;
    }
    
    void randomize_terrain_white(){
        for (int i=0; i < this.width; i++)
            for (int j=0; j < this.height; j++)
                this.cells[i, j].GetComponent<Cell>().set_height(Random.Range(1,5));
        this.redistribute_water();
    }
    
    void randomize_terrain_sin(float scale){
        float seed_x = Random.Range(0,1);
        float seed_y = Random.Range(0,1);
        
        for (int i=0; i < this.width; i++)
            for (int j=0; j < this.height; j++){
                int height = 1 + (int) (0.5 * Mathf.Abs(Mathf.Sin((seed_x+i) *scale)*5 + Mathf.Sin((seed_y+j) *scale) * 5));
                Debug.Log(height);
                this.cells[i, j].GetComponent<Cell>().set_height(height);

            }
        this.redistribute_water();
    }
    

}